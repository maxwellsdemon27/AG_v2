using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace Utils
{
    class HyperParameters
    {
        public static double max_course_error = 15;
        public static double max_position_error = 2;
        public static int n_ship_group = 3;
        public static string feature_type = "position";
        // public static string feature_type = "vector";
        public static int top_n_candidate = 1;
    }

    public class FormationPredictor
    {
        private List<ShipPermutation> ship_permutations = Functions.get_ship_permutations();

        public (List<ShipPermutation>, List<ShipPermutation>) predict(ShipPermutation sp_testing, double current_course = 0)
        {
            double rotation_angle = -current_course;
            sp_testing.apply_course_rotation(rotation_angle);

            List<ShipPermutation> sp_candidates, sp_predictions;
            (sp_candidates, sp_predictions) = this.predict_template(sp_testing);
            (sp_candidates, sp_predictions) = this.predict_position(sp_candidates, sp_predictions);

            foreach (ShipPermutation sp_prediction in sp_predictions)
            {
                sp_prediction.apply_course_rotation(-rotation_angle);
            }

            return (sp_candidates, sp_predictions);
        }

        private (List<ShipPermutation>, List<ShipPermutation>) predict_template(ShipPermutation sp_testing)
        {
            List<int> sorted_indexes = null;
            switch (HyperParameters.feature_type)
            {
                case "position":
                    List<double> dists = (from sp_template in this.ship_permutations select this.calculate_distance(sp_testing, sp_template)).ToList();
                    sorted_indexes = this.argsort(dists, descend_order: false);
                    break;
                case "vector":
                    List<double> sims = (from sp_template in this.ship_permutations select this.calculate_similarity(sp_testing, sp_template)).ToList();
                    sorted_indexes = this.argsort(sims, descend_order: true);
                    break;
            }

            List<ShipPermutation> sp_candidates = new List<ShipPermutation>();
            List<ShipPermutation> sp_predictions = new List<ShipPermutation>();
            foreach (int i in sorted_indexes.GetRange(0, HyperParameters.top_n_candidate))
            {
                ShipPermutation sp_candidate = this.ship_permutations[i];
                ShipPermutation sp_prediction = sp_testing.deep_copy();
                sp_prediction.formation = sp_candidate.formation;
                sp_candidates.Add(sp_candidate);
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
                sp_prediction.ship_position_predict = new Dictionary<string, Position>();
                for (int i = 0; i < sp_prediction.ships.Count; i++)
                {
                    sp_prediction.ship_position_predict.Add(sp_candidate.ships[i].name, sp_prediction.ships[i].p_final);
                }

                Dictionary<string, List<Position>> ships_positions = new Dictionary<string, List<Position>>();
                for (int i = 0; i < sp_prediction.ships.Count; i++)
                {
                    Ship ship_template = sp_candidate.ships[i];
                    Ship ship_detected = sp_prediction.ships[i];
                    foreach (KeyValuePair<string, Vector2> item in ship_template.ship_vectors)
                    {
                        string ship_name = item.Key;
                        if (!sp_prediction.ship_position_predict.ContainsKey(ship_name))
                        {
                            Position p_predict = new Position(ship_detected.p_final.x + item.Value.X, ship_detected.p_final.y + item.Value.Y);
                            if (!ships_positions.ContainsKey(ship_name))
                                ships_positions.Add(ship_name, new List<Position> { p_predict });
                            else
                                ships_positions[ship_name].Add(p_predict);
                        }
                    }
                }

                foreach (KeyValuePair<string, List<Position>> item in ships_positions)
                {
                    var x_values = from p in item.Value select p.x;
                    var y_values = from p in item.Value select p.y;
                    sp_prediction.ship_position_predict.Add(item.Key, new Position(x_values.Average(), y_values.Average()));
                }
            }

            return (sp_candidates, sp_predictions);
        }
        private double calculate_similarity(ShipPermutation sp1, ShipPermutation sp2)
        {
            double sim = 0;
            for (int i = 0; i < sp1.feature.Count; i++)
            {
                if (!length_threshold(sp1.feature[i], sp2.feature[i]))
                {
                    return 0;
                }
                sim += cosine_similarity(sp1.feature[i], sp2.feature[i]);
            }
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
            return Math.Abs(norm(v1) - norm(v2)) > HyperParameters.max_position_error * 2 ? false : true;
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

        private double distance(Vector2 p1, Vector2 p2)
        {
            return Math.Pow(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2), 0.5);
        }

        private List<int> argsort(List<double> values, bool descend_order = false)
        {
            List<double> values_cp = new List<double>(values);
            values_cp.Sort();

            List<int> sorted_indexes = (from value in values_cp select values.IndexOf(value)).ToList();
            if (descend_order)
            {
                sorted_indexes.Reverse();
            }
            return sorted_indexes;
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
        public Position p, p_final;
        public string name;
        public Dictionary<string, Vector2> ship_vectors;
        public Ship(double x, double y, string name = null)
        {
            this.p = new Position(x, y);
            this.p_final = new Position(x, y);
            this.name = name;
        }

        public Ship deep_copy()
        {
            Ship other = (Ship)this.MemberwiseClone();
            other.p_final = new Position(this.p_final.x, this.p_final.y);
            return other;
        }

        public void rotate_coordinate(double angle)
        {
            (this.p_final.x, this.p_final.y) = Functions.rotate(this.p_final.x, this.p_final.y, angle);
        }
    }

    public class ShipPermutation
    {
        public string formation = null;
        public List<Ship> formation_data;
        public int sp_index;
        public List<int> ship_indexes;

        public List<Ship> ships;
        private List<Vector2> _feature = null;
        public Dictionary<string, Position> ship_position_predict = null;
        public ShipPermutation(string mode, string formation = null, List<Ship> formation_data = null, int sp_index = -1, List<int> ship_indexes = null, List<Ship> ships = null)
        {
            switch (mode)
            {
                case "experiment":
                    this.formation = formation;
                    this.formation_data = formation_data;
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
            var x_values = from ship in this.ships select ship.p_final.x;
            var y_values = from ship in this.ships select ship.p_final.y;

            return (x_values.Average(), y_values.Average());
        }

        private void calculate_position_feature()
        {
            (double x_offset, double y_offset) = this.calculate_position_offset();
            this.feature = (from ship in this.ships select new Vector2((float)(ship.p_final.x - x_offset), (float)(ship.p_final.y - y_offset))).ToList();
        }

        private void calculate_vector_feature()
        {
            List<Vector2> feature = new List<Vector2>();
            double x_start = this.ships[0].p_final.x;
            double y_start = this.ships[0].p_final.y;

            for (int i = 1; i < this.ships.Count; i++)
            {
                double x_end = this.ships[i].p_final.x;
                double y_end = this.ships[i].p_final.y;
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
            }
        }

        public void apply_course_rotation(double angle)
        {
            if (this.ship_position_predict == null)
            {
                foreach (Ship ship in this.ships) { ship.rotate_coordinate(angle); }
                this.calculate_feature();
            }
            else
            {
                foreach (KeyValuePair<string, Position> item in this.ship_position_predict)
                {
                    (item.Value.x, item.Value.y) = Functions.rotate(item.Value.x, item.Value.y, angle);
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
                ship.rotate_coordinate(course_rotation_angle);

                // position offset
                double unit_vector_x = random.NextDouble() * HyperParameters.max_position_error;
                // double unit_vector_x = HyperParameters.max_position_error;
                double unit_vector_y = 0;
                double position_offset_angle = random.NextDouble() * 360;
                (unit_vector_x, unit_vector_y) = Functions.rotate(unit_vector_x, unit_vector_y, position_offset_angle);
                ship.p_final.x += unit_vector_x;
                ship.p_final.y += unit_vector_y;
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
                    new Ship(    0,     0, "CVLL"),
                    new Ship(-37.6,  13.7, "A1"),
                    new Ship(-37.6, -13.7, "A2"),
                    new Ship( -6.8,  18.8, "C1"),
                    new Ship( -6.8, -18.8, "C2"),
                    new Ship(  -20,     0, "D")}},
                {"to_land", new List<Ship>{
                    new Ship(    0,     0, "CVLL"),
                    new Ship(-17.3,    10, "A1"),
                    new Ship(-17.3,   -10, "A2"),
                    new Ship( -3.5,  19.7, "C1"),
                    new Ship( -3.5, -19.7, "C2"),
                    new Ship(  -20,     0, "D")}},
                {"anti_aircraft", new List<Ship>{
                    new Ship(    0,     0, "CVLL"),
                    new Ship(-17.3,    10, "A1"),
                    new Ship(-17.3,   -10, "A2"),
                    new Ship( -6.9,  39.4, "C1"),
                    new Ship( -6.9, -39.4, "C2"),
                    new Ship(   40,     0, "D")}},
                {"anti_submarine", new List<Ship>{
                    new Ship(    0,     0, "CVLL"),
                    new Ship(-18.1,   8.5, "A1"),
                    new Ship(-18.1,  -8.5, "A2"),
                    new Ship(  3.5,  19.7, "C1"),
                    new Ship(  3.5, -19.7, "C2"),
                    new Ship(   20,     0, "D")}}
            };

            foreach (KeyValuePair<string, List<Ship>> item in database)
            {
                foreach (Ship ship_start in item.Value)
                {
                    Dictionary<string, Vector2> ship_vectors = new Dictionary<string, Vector2>();
                    foreach (Ship ship_end in item.Value)
                    {
                        ship_vectors.Add(ship_end.name, new Vector2((float)(ship_end.p_final.x - ship_start.p_final.x), (float)(ship_end.p_final.y - ship_start.p_final.y)));
                    }
                    ship_start.ship_vectors = ship_vectors;
                }
            }
            return database;
        }

        public static List<ShipPermutation> get_ship_permutations()
        {
            List<ShipPermutation> ship_permutations = new List<ShipPermutation>();
            int sp_index = 0;
            foreach (KeyValuePair<string, List<Ship>> item in get_database())
            {
                string formation = item.Key;
                List<Ship> formation_data = item.Value;

                foreach (List<int> ship_indexes in Functions.permute_index(Enumerable.Range(1, formation_data.Count - 1).ToList(), HyperParameters.n_ship_group))
                {
                    sp_index += 1;
                    ship_permutations.Add(new ShipPermutation("experiment", formation, formation_data, sp_index, ship_indexes));
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

        public static (float, float) rotate(double x, double y, double angle)
        {
            double radian = angle / 180 * Math.PI;
            double rotated_x = x * Math.Cos(radian) - y * Math.Sin(radian);
            double rotated_y = x * Math.Sin(radian) + y * Math.Cos(radian);
            return ((float)rotated_x, (float)rotated_y);
        }
    }
}
