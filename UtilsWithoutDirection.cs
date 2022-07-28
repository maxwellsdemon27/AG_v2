using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace UtilsWithoutDirection
{
    class HyperParameters
    {
        public static int n_simulate = 1000;
        public static double max_course_error = 15;
        public static double max_position_error = 2;
        // public static string feature_type = "position";
        // public static string feature_type = "vector";
        public static string feature_type = "angle";
        public static int top_n_candidate = 3;
        public static int top_n_predictions = 3;
        public static double initial_course = 90;
    }

    public class FormationPredictor
    {
        private Dictionary<int, List<ShipPermutation>> all_ship_permutations = new Dictionary<int, List<ShipPermutation>>();
        private List<ShipPermutation> current_ship_permutations;

        public FormationPredictor()
        {
            foreach (int n_ship_group in Enumerable.Range(3, 3))
            {
                this.all_ship_permutations.Add(n_ship_group, Functions.get_ship_permutations(n_ship_group));
            }
        }

        public (List<ShipPermutation>, List<ShipPermutation>) predict(ShipPermutation sp_testing, double current_course = 0)
        {
            this.current_ship_permutations = this.all_ship_permutations[sp_testing.ships.Count];

            double rotation_angle = -current_course;
            if (current_course != 0)
            {
                sp_testing.apply_course_rotation(Functions.angle2radian(rotation_angle));
            }

            List<ShipPermutation> sp_candidates, sp_predictions;
            (sp_candidates, sp_predictions) = this.predict_template3(sp_testing);
            (sp_candidates, sp_predictions) = this.predict_position(sp_candidates, sp_predictions);
            (sp_candidates, sp_predictions) = this.prediction_resort(sp_candidates, sp_predictions);

            sp_candidates = sp_candidates.Take(HyperParameters.top_n_predictions).ToList();
            sp_predictions = sp_predictions.Take(HyperParameters.top_n_predictions).ToList();

            if (current_course != 0)
            {
                foreach (ShipPermutation sp_prediction in sp_predictions)
                {
                    sp_prediction.apply_course_rotation(Functions.angle2radian(-rotation_angle));
                }
            }

            if (HyperParameters.feature_type == "angle")
            {
                foreach (ShipPermutation sp_prediction in sp_predictions)
                {
                    sp_prediction.apply_course_rotation(sp_prediction.predicted_radian);
                }
            }

            return (sp_candidates, sp_predictions);
        }

        private (List<ShipPermutation>, List<ShipPermutation>) predict_template(ShipPermutation sp_testing)
        {
            List<int> sorted_indexes = null;

            switch (HyperParameters.feature_type)
            {
                case "position":
                    List<double> dists = (from sp_template in this.current_ship_permutations select this.calculate_distance(sp_testing, sp_template)).ToList();
                    sorted_indexes = this.argsort(dists, descend_order: false);
                    break;
                case "vector":
                    List<double> sims = (from sp_template in this.current_ship_permutations select this.calculate_similarity(sp_testing, sp_template)).ToList();
                    sorted_indexes = this.argsort(sims, descend_order: true);
                    break;
                case "angle":
                    List<double> diffs = (from sp_template in this.current_ship_permutations select this.calculate_angle_diff(sp_testing, sp_template)).ToList();
                    sorted_indexes = this.argsort(diffs, descend_order: false);
                    break;
            }

            List<ShipPermutation> sp_candidates = new List<ShipPermutation>();
            List<ShipPermutation> sp_predictions = new List<ShipPermutation>();
            foreach (int i in sorted_indexes.GetRange(0, HyperParameters.top_n_candidate))
            {
                ShipPermutation sp_candidate = this.current_ship_permutations[i];
                ShipPermutation sp_prediction = sp_testing.deep_copy();
                sp_prediction.formation = sp_candidate.formation;
                sp_prediction.sp_index = sp_candidate.sp_index;
                sp_candidates.Add(sp_candidate);
                sp_predictions.Add(sp_prediction);
            }

            return (sp_candidates, sp_predictions);
        }

        private (List<ShipPermutation>, List<ShipPermutation>) predict_template3(ShipPermutation sp_testing)
        {
            List<ShipPermutation> sp_candidates = new List<ShipPermutation>();

            foreach (ShipPermutation sp_template in this.current_ship_permutations)
            {
                if (!this.check_CVLL(sp_testing, sp_template))
                    continue;
                if (!this.check_vector_length(sp_testing, sp_template))
                    continue;
                sp_candidates.Add(sp_template);
            }

            List<ShipPermutation> sp_predictions = new List<ShipPermutation>();
            foreach (ShipPermutation sp_candidate in sp_candidates)
            {
                ShipPermutation sp_prediction = sp_testing.deep_copy();
                sp_prediction.formation = sp_candidate.formation;
                sp_prediction.sp_index = sp_candidate.sp_index;
                sp_predictions.Add(sp_prediction);
            }

            return (sp_candidates, sp_predictions);
        }

        private (List<ShipPermutation>, List<ShipPermutation>) predict_position(List<ShipPermutation> sp_candidates, List<ShipPermutation> sp_predictions)
        {
            for (int nth_sp = 0; nth_sp < sp_candidates.Count; nth_sp++)
            {
                ShipPermutation sp_candidate = sp_candidates[nth_sp];
                ShipPermutation sp_prediction = sp_predictions[nth_sp];

                if (HyperParameters.feature_type == "angle")
                {
                    Vector2 cv1 = this.calculate_center_vector(sp_candidate);
                    Vector2 cv2 = this.calculate_center_vector(sp_prediction);
                    sp_prediction.predicted_radian = this.radian_between_vector(cv1, cv2);
                    sp_prediction.apply_course_rotation(-sp_prediction.predicted_radian);
                }

                sp_prediction.ship_position_predict = new Dictionary<string, Position>();
                for (int i = 0; i < sp_prediction.ships.Count; i++)
                {
                    sp_prediction.ship_position_predict.Add(sp_candidate.ships[i].name, sp_prediction.ships[i].position);
                }

                foreach (string ship_name in sp_candidate.formation_data.Keys)
                {
                    if (!sp_prediction.ship_position_predict.Keys.Contains(ship_name))
                    {
                        List<Position> ship_positions = new List<Position>();
                        for (int i = 0; i < sp_candidate.ships.Count; i++)
                        {
                            Ship ship_template = sp_candidate.ships[i];
                            Ship ship_detected = sp_prediction.ships[i];
                            Vector2 ship_vector = ship_template.ship_vectors[ship_name];
                            Position p_predict = new Position(ship_detected.position.x + ship_vector.X, ship_detected.position.y + ship_vector.Y);
                            ship_positions.Add(p_predict);
                        }
                        var x_values = (from p in ship_positions select p.x);
                        var y_values = (from p in ship_positions select p.y);
                        sp_prediction.ship_position_predict[ship_name] = new Position(x_values.Average(), y_values.Average());
                    }
                }
            }

            return (sp_candidates, sp_predictions);
        }

        private (List<ShipPermutation>, List<ShipPermutation>) prediction_resort(List<ShipPermutation> sp_candidates, List<ShipPermutation> sp_predictions)
        {
            List<double> dists = new List<double>();
            for (int i = 0; i < sp_candidates.Count; i++)
            {
                ShipPermutation sp_candidate = sp_candidates[i];
                ShipPermutation sp_prediction = sp_predictions[i];

                // double x_center_prediction = sp_prediction.ship_position_predict["CVLL"].x;
                // double y_center_prediction = sp_prediction.ship_position_predict["CVLL"].y;

                double x_center_prediction = (from p in sp_prediction.ship_position_predict.Values select p.x).Average();
                double y_center_prediction = (from p in sp_prediction.ship_position_predict.Values select p.y).Average();
                double x_center_candidate = (from p in sp_candidate.formation_data.Values select p.x).Average();
                double y_center_candidate = (from p in sp_candidate.formation_data.Values select p.y).Average();

                double dist = 0;
                foreach (KeyValuePair<string, Position> item in sp_prediction.ship_position_predict)
                {
                    string ship_name = item.Key;
                    Position p = item.Value;
                    Vector2 p1 = new Vector2((float)(p.x - x_center_prediction), (float)(p.y - y_center_prediction));
                    // Vector2 p2 = new Vector2((float)(sp_candidate.formation_data[ship_name].x), (float)(sp_candidate.formation_data[ship_name].y));
                    Vector2 p2 = new Vector2((float)(sp_candidate.formation_data[ship_name].x - x_center_candidate), (float)(sp_candidate.formation_data[ship_name].y - y_center_candidate));
                    dist += this.distance(p1, p2);
                }
                dists.Add(dist);
            }
            List<int> indexes = this.argsort(dists);

            List<ShipPermutation> sp_candidates_resort = (from i in indexes select sp_candidates[i]).ToList();
            List<ShipPermutation> sp_predictions_resort = (from i in indexes select sp_predictions[i]).ToList();

            return (sp_candidates_resort, sp_predictions_resort);
        }

        private bool check_CVLL(ShipPermutation sp1, ShipPermutation sp2)
        {
            var names1 = (from ship in sp1.ships select ship.name).ToList();
            var names2 = (from ship in sp2.ships select ship.name).ToList();
            var index1 = names1.IndexOf("CVLL");
            var index2 = names2.IndexOf("CVLL");
            return index1 == index2 ? true : false;
        }

        private bool check_vector_length(ShipPermutation sp1, ShipPermutation sp2)
        {
            for (int i = 0; i < sp1.feature.Count; i++)
            {
                Vector2 f1 = sp1.feature[i];
                Vector2 f2 = sp2.feature[i];
                if (!this.length_threshold(f1, f2))
                {
                    return false;
                }
            }
            return true;
        }

        private double calculate_angle_diff(ShipPermutation sp1, ShipPermutation sp2)
        {
            if (!(this.check_CVLL(sp1, sp2) && this.check_vector_length(sp1, sp2)))
            {
                return Double.PositiveInfinity;
            }

            double diff = 0;
            for (int i = 1; i < sp1.feature.Count; i++)
            {
                double theta1 = Math.Round(this.radian_between_vector(sp1.feature[0], sp1.feature[i]), 5);
                double theta2 = Math.Round(this.radian_between_vector(sp2.feature[0], sp2.feature[i]), 5);
                diff += Math.Abs(theta1 - theta2);
            }

            return diff;
        }

        private double calculate_similarity(ShipPermutation sp1, ShipPermutation sp2)
        {
            if (!this.check_vector_length(sp1, sp2))
            {
                return 0;
            }

            double sim = 0;
            for (int i = 0; i < sp1.feature.Count; i++) { sim += cosine_similarity(sp1.feature[i], sp2.feature[i]); }
            return sim;
        }

        private double calculate_distance(ShipPermutation sp1, ShipPermutation sp2)
        {
            double sum = 0;
            for (int i = 0; i < sp1.feature.Count; i++) { sum += distance(sp1.feature[i], sp2.feature[i]); }
            return sum;
        }

        private bool length_threshold(Vector2 v1, Vector2 v2)
        {
            return Math.Round(Math.Abs(norm(v1) - norm(v2)), 5) <= HyperParameters.max_position_error * 2 ? true : false;
        }

        private double cosine_similarity(Vector2 v1, Vector2 v2)
        {
            return dot(v1, v2) / (norm(v1) * norm(v2));
        }

        private double norm(Vector2 v)
        {
            return Math.Pow(Math.Pow(v.X, 2) + Math.Pow(v.Y, 2), 0.5);
        }

        private double dot(Vector2 v1, Vector2 v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y;
        }

        private double det(Vector2 v1, Vector2 v2)
        {
            return v1.X * v2.Y - v2.X * v1.Y;
        }

        private double radian_between_vector(Vector2 v1, Vector2 v2)
        {
            return Math.Atan2(this.det(v1, v2), this.dot(v1, v2));
        }

        private Vector2 calculate_center_vector(ShipPermutation sp)
        {
            var radians = new List<double>();
            for (int i = 0; i < sp.feature.Count; i++)
            {
                radians.Add(this.radian_between_vector(sp.feature[0], sp.feature[i]));
            }
            (double x, double y) = Functions.rotate(sp.feature[0].X, sp.feature[0].Y, radians.Average());
            return new Vector2((float)x, (float)y);
        }

        private double distance(Vector2 p1, Vector2 p2)
        {
            return Math.Pow(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2), 0.5);
        }

        private List<int> argsort(List<double> values, bool descend_order = false)
        {
            var sorted = values
                .Select((x, i) => new KeyValuePair<double, int>(x, i))
                .OrderBy(x => x.Key)
                .ToList();

            List<int> sorted_index = sorted.Select(x => x.Value).ToList();

            if (descend_order)
            {
                sorted_index.Reverse();
            }

            return sorted_index;
        }
    }

    public class Position
    {
        public double x, y;
        public Position(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class Ship
    {
        public Position position;
        public string name;
        public Dictionary<string, Vector2> ship_vectors;
        public Ship(double angle = 0, double distance = 0, double x = 0, double y = 0, string name = null, string position_type = "angle_distance")
        {
            if (position_type == "angle_distance")
            {
                (x, y) = Functions.rotate(distance, 0, Functions.angle2radian(HyperParameters.initial_course + angle));
            }
            this.position = new Position(x, y);
            this.name = name;
        }

        public Ship deep_copy()
        {
            Ship other = (Ship)this.MemberwiseClone();
            other.position = new Position(this.position.x, this.position.y);
            return other;
        }

        public void rotate_coordinate(double radian)
        {
            (this.position.x, this.position.y) = Functions.rotate(this.position.x, this.position.y, radian);
        }
    }

    public class ShipPermutation
    {
        public string formation = null;
        public Dictionary<string, Position> formation_data;
        public int sp_index;
        public List<int> ship_indexes;

        public List<Ship> ships;
        private List<Vector2> _feature = null;
        public Dictionary<string, Position> ship_position_predict = null;
        public double predicted_radian;
        public ShipPermutation(string mode, string formation = null, List<Ship> formation_data = null, int sp_index = -1, List<int> ship_indexes = null, List<Ship> ships = null)
        {
            switch (mode)
            {
                case "experiment":
                    this.formation = formation;
                    this.formation_data = (from ship in formation_data select ship).ToDictionary(k => k.name, v => v.position);
                    this.sp_index = sp_index;
                    this.ship_indexes = ship_indexes;
                    this.ships = (from i in this.ship_indexes select formation_data[i]).ToList();
                    break;
                case "inference":
                    this.ships = ships;
                    break;
            }
        }

        public ShipPermutation deep_copy()
        {
            ShipPermutation other = (ShipPermutation)this.MemberwiseClone();
            other.ships = (from ship in this.ships select ship.deep_copy()).ToList();
            return other;
        }

        public List<Vector2> feature
        {
            get
            {
                if (this._feature == null)
                {
                    this.calculate_feature();
                }
                return this._feature;
            }
            set { this._feature = value; }
        }

        private (double, double) calculate_position_offset()
        {
            var x_values = from ship in this.ships select ship.position.x;
            var y_values = from ship in this.ships select ship.position.y;

            return (x_values.Average(), y_values.Average());
        }

        private void calculate_position_feature()
        {
            (double x_offset, double y_offset) = this.calculate_position_offset();
            this.feature = (from ship in this.ships select new Vector2((float)(ship.position.x - x_offset), (float)(ship.position.y - y_offset))).ToList();
        }

        private void calculate_vector_feature()
        {
            List<Vector2> feature = new List<Vector2>();
            double x_start = this.ships[0].position.x;
            double y_start = this.ships[0].position.y;

            for (int i = 1; i < this.ships.Count; i++)
            {
                double x_end = this.ships[i].position.x;
                double y_end = this.ships[i].position.y;
                feature.Add(new Vector2(((float)(x_end - x_start)), (float)(y_end - y_start)));
            }

            this.feature = feature;
        }

        private void calculate_feature()
        {
            switch (HyperParameters.feature_type)
            {
                case "position":
                    this.calculate_position_feature();
                    break;
                case "vector":
                    this.calculate_vector_feature();
                    break;
                case "angle":
                    this.calculate_vector_feature();
                    break;
            }
        }

        public void apply_course_rotation(double radian)
        {
            if (this.ship_position_predict == null)
            {
                foreach (Ship ship in this.ships) { ship.rotate_coordinate(radian); }
                this.calculate_feature();
            }
            else
            {
                foreach (KeyValuePair<string, Position> item in this.ship_position_predict)
                {
                    (item.Value.x, item.Value.y) = Functions.rotate(item.Value.x, item.Value.y, radian);
                }
            }
        }

        public void apply_sp_offset()
        {
            Random random = new Random();
            double course_rotation_angle = HyperParameters.max_course_error * (2 * random.NextDouble() - 1);
            // double course_rotation_angle = new List<double> { -HyperParameters.max_course_error, HyperParameters.max_course_error }[random.Next(2)];

            foreach (Ship ship in this.ships)
            {
                // course offset
                ship.rotate_coordinate(Functions.angle2radian(course_rotation_angle));

                // position offset
                double unit_vector_x = random.NextDouble() * HyperParameters.max_position_error;
                // double unit_vector_x = HyperParameters.max_position_error;
                double unit_vector_y = 0;
                double position_offset_angle = random.NextDouble() * 360;
                (unit_vector_x, unit_vector_y) = Functions.rotate(unit_vector_x, unit_vector_y, Functions.angle2radian(position_offset_angle));
                ship.position.x += unit_vector_x;
                ship.position.y += unit_vector_y;
            }

            this.calculate_feature();
        }
    }

    class Functions
    {
        public static Dictionary<string, List<Ship>> get_database()
        {
            Dictionary<string, List<Ship>> database = new Dictionary<string, List<Ship>>
            {
                {"anti_warship", new List<Ship>{
                    new Ship(   0,   0, name: "CVLL"),
                    new Ship(  20,  40, name: "A1"),
                    new Ship( -20,  40, name: "A2"),
                    new Ship(  70,  20, name: "C1"),
                    new Ship( -70,  20, name: "C2"),
                    new Ship(   0,  20, name: "D")}},
                {"to_land", new List<Ship>{
                    new Ship(   0,   0, name: "CVLL"),
                    new Ship(  30,  20, name: "A1"),
                    new Ship( -30,  20, name: "A2"),
                    new Ship(  80,  20, name: "C1"),
                    new Ship( -80,  20, name: "C2"),
                    new Ship(   0,  20, name: "D")}},
                {"anti_aircraft", new List<Ship>{
                    new Ship(   0,   0, name: "CVLL"),
                    new Ship(  30,  20, name: "A1"),
                    new Ship( -30,  20, name: "A2"),
                    new Ship(  80,  40, name: "C1"),
                    new Ship( -80,  40, name: "C2"),
                    new Ship( 180,  40, name: "D")}},
                {"anti_submarine", new List<Ship>{
                    new Ship(   0,   0, name: "CVLL"),
                    new Ship(  25,  20, name: "A1"),
                    new Ship( -25,  20, name: "A2"),
                    new Ship( 100,  20, name: "C1"),
                    new Ship(-100,  20, name: "C2"),
                    new Ship( 180,  20, name: "D")}}
            };

            foreach (KeyValuePair<string, List<Ship>> item in database)
            {
                foreach (Ship ship_start in item.Value)
                {
                    Dictionary<string, Vector2> ship_vectors = new Dictionary<string, Vector2>();
                    foreach (Ship ship_end in item.Value)
                    {
                        ship_vectors.Add(ship_end.name, new Vector2((float)(ship_end.position.x - ship_start.position.x), (float)(ship_end.position.y - ship_start.position.y)));
                    }
                    ship_start.ship_vectors = ship_vectors;
                }
            }
            return database;
        }

        public static List<ShipPermutation> get_ship_permutations(int n_ships)
        {
            List<ShipPermutation> ship_permutations = new List<ShipPermutation>();
            int sp_index = 0;
            foreach (KeyValuePair<string, List<Ship>> item in get_database())
            {
                string formation = item.Key;
                List<Ship> formation_data = item.Value;

                int start_index = 0;
                int count = formation_data.Count - start_index;
                foreach (List<int> ship_indexes in Functions.permute_index(Enumerable.Range(start_index, count).ToList(), n_ships))
                {
                    ship_permutations.Add(new ShipPermutation("experiment", formation, formation_data, sp_index, ship_indexes));
                    sp_index += 1;
                }
            }
            return ship_permutations;
        }

        private static List<List<int>> permute_index(List<int> indexes, int n)
        {
            List<List<int>> perms = new List<List<int>>();

            if (n == 1)
            {
                perms = (from index in indexes select new List<int> { index }).ToList();
                return perms;
            }

            foreach (int index in indexes)
            {
                List<int> cp = new List<int>(indexes);
                cp.Remove(index);
                foreach (List<int> p in permute_index(cp, n - 1))
                {
                    List<int> perm = new List<int>() { index };
                    perm.AddRange(p);
                    perms.Add(perm);
                }
            }
            return perms;
        }

        public static ShipPermutation generate_testing_sample(List<ShipPermutation> testing_database)
        {
            ShipPermutation sp_testing = testing_database[new Random().Next(testing_database.Count)].deep_copy();
            sp_testing.apply_sp_offset();
            return sp_testing;
        }

        public static (double, double) rotate(double x, double y, double radian)
        {
            double rotated_x = x * Math.Cos(radian) - y * Math.Sin(radian);
            double rotated_y = x * Math.Sin(radian) + y * Math.Cos(radian);
            return (rotated_x, rotated_y);
        }

        public static double angle2radian(double angle)
        {
            return angle / 180 * Math.PI;
        }
    }
}
