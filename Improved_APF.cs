using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using DubinsPathsTutorial;
using UnityEngine;
using Utils;

namespace ImprovedAPF
{
    public class Improved_APF : MonoBehaviour
    {
        /*
        :param start: 起点
        :param goal: 终点
        :param obstacles: 障碍物列表，每个元素为Vector2d对象
        :param k_att: 引力系数
        :param k_rep: 斥力系数
        :param rr: 斥力作用范围
        :param step_size: 步长
        :param max_iters: 最大迭代次数
        :param goal_threshold: 离目标点小于此值即认为到达目标点
        */
        public System.Numerics.Vector2 start = new System.Numerics.Vector2(0, 0);
        public System.Numerics.Vector2 current_pos = new System.Numerics.Vector2(0, 0);
        public System.Numerics.Vector2 goal = new System.Numerics.Vector2(0, 0);
        public List<Tuple<System.Numerics.Vector2, float>> obstacles;
        public float k_att = 0.005f;
        public float k_rep = 0.0f;
        public float step_size = 0.2f;
        public int max_iters = 2000;
        public int iters = 0;
        public float goal_threshold = 0.2f;

        List<System.Numerics.Vector2> path = new List<System.Numerics.Vector2>();
        public bool is_path_plan_success = false;

        public Improved_APF(System.Numerics.Vector2 start, System.Numerics.Vector2 goal, List<Tuple<System.Numerics.Vector2, float>> obstacles, float step_size, int max_iters, float goal_threshold, float k_att, float k_rep = 28.0f)
        {
            this.start = new System.Numerics.Vector2(start.X, start.Y);
            this.current_pos = new System.Numerics.Vector2(start.X, start.Y);
            this.goal = new System.Numerics.Vector2(goal.X, goal.Y);
            this.obstacles = obstacles;
            this.k_att = k_att;
            this.k_rep = k_rep;
            this.step_size = step_size;
            this.max_iters = max_iters;
            this.goal_threshold = goal_threshold;
        }

        public System.Numerics.Vector2 attractive()
        {
            /*
            引力计算
            :return: 引力
            */
            System.Numerics.Vector2 att = System.Numerics.Vector2.Subtract(this.goal, this.current_pos) * this.k_att;
            return att;
        }

        public System.Numerics.Vector2 repulsion()
        {
            /*
            斥力计算, 改进斥力函数, 解决不可达问题
            :return: 斥力大小
            */
            System.Numerics.Vector2 rep = new System.Numerics.Vector2(0, 0);
            for (int i = 0; i < obstacles.Count; i++)
            {
                Tuple<System.Numerics.Vector2, float> obstacle = obstacles[i];
                float dist = System.Numerics.Vector2.Distance(this.current_pos, obstacle.Item1);
                System.Numerics.Vector2 obs_to_rob = System.Numerics.Vector2.Subtract(current_pos, obstacle.Item1);
                System.Numerics.Vector2 rob_to_goal = System.Numerics.Vector2.Subtract(goal, current_pos);
                if (obs_to_rob.Length() <= obstacle.Item2)
                {
                    if (obstacle.Item2 == 15.0f)
                    {
                        k_rep = A_threat_value(dist);
                    }
                    else
                    {
                        k_rep = CD_threat_value(dist);

                    }

                    // 威脅給我的放射性推力
                    System.Numerics.Vector2 rep_1 = System.Numerics.Vector2.Normalize(obs_to_rob)
                                    * k_rep
                                    * (1.0f / obs_to_rob.Length() - 1.0f / obstacle.Item2)
                                    / (Mathf.Pow(obs_to_rob.Length(), 2))
                                    * (Mathf.Pow(rob_to_goal.Length(), 2));

                    System.Numerics.Vector2 rep_2 = System.Numerics.Vector2.Normalize(rob_to_goal)
                                    * k_rep
                                    * (Mathf.Pow((1.0f / obs_to_rob.Length() - 1.0f / obstacle.Item2), 2))
                                    * rob_to_goal.Length();

                    System.Numerics.Vector2 rep_sum = System.Numerics.Vector2.Add(rep_1, rep_2);
                    rep = System.Numerics.Vector2.Add(rep, rep_sum);
                }
            }
            return rep;
        }

        public float A_threat_value(float dist)
        {
            float threat;
            if (dist >= 15.0f) threat = 0;
            else if (dist >= 3.5f && dist < 15.0f) threat = -0.0173913f * dist + 0.26086957f;
            else if (dist >= 2.5f && dist < 3.5f) threat = -0.5f * dist + 1.95f;
            else if (dist >= 0.5f && dist < 2.5f) threat = 0.7f;
            else threat = 1.4f * dist + 0.0f;

            return threat;
        }

        public float CD_threat_value(float dist)
        {
            float threat;
            if (dist >= 28.0f) threat = 0;
            else if (dist >= 17.0f && dist < 28.0f) threat = -0.01818182f * dist + 0.50909091f;
            else if (dist >= 8.0f && dist < 17.0f) threat = -0.03333333f * dist + 0.76666667f;
            else if (dist >= 2.5f && dist < 8.0f) threat = -0.03636364f * dist + 0.79090909f;
            else if (dist >= 0.5f && dist < 2.5f) threat = 0.7f;
            else threat = 1.4f * dist + 0.0f;

            return threat;
        }

        public List<System.Numerics.Vector2> path_plan()
        {
            while (this.iters < this.max_iters && System.Numerics.Vector2.Subtract(this.current_pos, this.goal).Length() > this.goal_threshold)
            {
                System.Numerics.Vector2 F_vec = System.Numerics.Vector2.Add(this.attractive(), this.repulsion());
                this.current_pos = System.Numerics.Vector2.Add(this.current_pos, System.Numerics.Vector2.Multiply(System.Numerics.Vector2.Normalize(F_vec), step_size));
                path.Add(current_pos);
                this.iters += 1;
            }
            if (System.Numerics.Vector2.Subtract(this.current_pos, this.goal).Length() <= this.goal_threshold)
            {
                this.is_path_plan_success = true;
            }
            return path;
        }

        public static Tuple<MathFunction.Circle, string, List<PointF>> Retun_Circle(PointF first_point, PointF second_point, PointF third_point, System.Numerics.Vector2 first_second_vec, System.Numerics.Vector2 second_third_vec)
        {
            List<PointF> waypoints = new List<PointF>();

            // 1:left, -1:right
            int turn_side = MathFunction.SideOfVector(first_point, second_point, third_point);
            // System.Numerics.Vector2 first_second_vec = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(x: second_point.X - first_point.X, y: second_point.Y - first_point.Y));
            float first_second_dist = (float)MathFunction.Distance(first_point, second_point);

            // System.Numerics.Vector2 second_third_vec = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(x: third_point.X - second_point.X, y: third_point.Y - second_point.Y));
            float second_third_dist = (float)MathFunction.Distance(second_point, third_point);

            System.Numerics.Vector2 normal_vec;
            if (turn_side == 1)
            {
                normal_vec = new System.Numerics.Vector2(x: -first_second_vec.Y, y: first_second_vec.X);
            }
            else
            {
                normal_vec = new System.Numerics.Vector2(x: first_second_vec.Y, y: -first_second_vec.X);
            }

            float theta = (float)MathFunction.Angle(o: second_point, s: first_point, e: third_point) / 2;

            float waypoint_second_dist = 7.225f / Mathf.Tan(theta / 180.0f * Mathf.PI);

            float waypoint_first_dist = first_second_dist - waypoint_second_dist;

            float waypoint_third_dist = second_third_dist - waypoint_second_dist;

            float waypoint1_x = (waypoint_second_dist * first_point.X + waypoint_first_dist * second_point.X) / first_second_dist;
            float waypoint1_y = (waypoint_second_dist * first_point.Y + waypoint_first_dist * second_point.Y) / first_second_dist;

            PointF waypoint1 = new PointF(waypoint1_x, waypoint1_y);

            float waypoint2_x = (waypoint_second_dist * third_point.X + waypoint_third_dist * second_point.X) / second_third_dist;
            float waypoint2_y = (waypoint_second_dist * third_point.Y + waypoint_third_dist * second_point.Y) / second_third_dist;

            PointF waypoint2 = new PointF(waypoint2_x, waypoint2_y);

            PointF return_center = new PointF(waypoint1.X + 7.225f * normal_vec.X, waypoint1.Y + 7.225f * normal_vec.Y);

            waypoints.Add(waypoint1);
            waypoints.Add(waypoint2);

            if (turn_side == 1)
            {
                return new Tuple<MathFunction.Circle, string, List<PointF>>(new MathFunction.Circle(return_center, 7.225f), "L", waypoints);
            }
            else
            {
                return new Tuple<MathFunction.Circle, string, List<PointF>>(new MathFunction.Circle(return_center, 7.225f), "R", waypoints);
            }


        }

        public static List<Tuple<MathFunction.Circle, string, List<PointF>>> Executable_Circle(List<System.Numerics.Vector2> down_sample_path)
        {
            List<Tuple<MathFunction.Circle, string, List<PointF>>> return_circles = new List<Tuple<MathFunction.Circle, string, List<PointF>>>();
            for (int i = 0; i < down_sample_path.Count - 2; i++)
            {
                PointF first_point = new PointF(down_sample_path[i].X, down_sample_path[i].Y);
                PointF second_point = new PointF(down_sample_path[i + 1].X, down_sample_path[i + 1].Y);
                PointF third_point = new PointF(down_sample_path[i + 2].X, down_sample_path[i + 2].Y);

                System.Numerics.Vector2 first_second_vec = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(x: second_point.X - first_point.X, y: second_point.Y - first_point.Y));
                System.Numerics.Vector2 second_third_vec = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(x: third_point.X - second_point.X, y: third_point.Y - second_point.Y));

                if (first_second_vec.Equals(second_third_vec) != true)
                {
                    return_circles.Add(Retun_Circle(first_point, second_point, third_point, first_second_vec, second_third_vec));
                }
                else
                {
                    down_sample_path.RemoveAt(i + 1);
                    i -= 1;
                }

            }

            List<Tuple<MathFunction.Circle, string, List<PointF>>> final_circles = new List<Tuple<MathFunction.Circle, string, List<PointF>>>();
            for (int i = 0; i < return_circles.Count - 1; i++)
            {
                if (return_circles[i].Item2 == return_circles[i + 1].Item2)
                {
                    float dist1 = (float)MathFunction.Distance(return_circles[i].Item3[0], return_circles[i].Item3[1]);
                    float dist2 = (float)MathFunction.Distance(return_circles[i].Item3[0], return_circles[i + 1].Item3[0]);
                    if (dist2 < dist1)
                    {
                        return_circles.RemoveAt(i + 1);
                        i -= 1;
                    }
                    else
                    {
                        final_circles.Add(new Tuple<MathFunction.Circle, string, List<PointF>>(return_circles[i].Item1, return_circles[i].Item2, return_circles[i].Item3));
                    }
                }
                else
                {
                    float dist = (float)MathFunction.Distance(return_circles[i].Item1.center, return_circles[i + 1].Item1.center);
                    if (dist < 14.45)
                    {
                        return_circles.RemoveAt(i + 1);
                        i -= 1;
                    }
                    else
                    {
                        final_circles.Add(new Tuple<MathFunction.Circle, string, List<PointF>>(return_circles[i].Item1, return_circles[i].Item2, return_circles[i].Item3));
                    }
                }

            }
            final_circles.Add(new Tuple<MathFunction.Circle, string, List<PointF>>(return_circles[return_circles.Count - 1].Item1, return_circles[return_circles.Count - 1].Item2, return_circles[return_circles.Count - 1].Item3));

            return final_circles;
        }

        public static List<Tuple<MathFunction.Circle, string, List<PointF>>> Executable_Circle_(List<System.Numerics.Vector2> down_sample_path, System.Drawing.PointF startPos_point, System.Numerics.Vector2 heading_vec)
        {
            System.Drawing.PointF heading_vec_second_point = new System.Drawing.PointF(x: startPos_point.X + heading_vec.X, y: startPos_point.Y + heading_vec.Y);
            // System.Drawing.PointF modified_first_APF_point = MathFunction.GetIntersection(startPos_point, heading_vec_second_point, new PointF(down_sample_path[0].X, down_sample_path[0].Y), new PointF(down_sample_path[1].X, down_sample_path[1].Y));

            down_sample_path.Insert(0, new System.Numerics.Vector2(startPos_point.X, startPos_point.Y));
            // down_sample_path.RemoveAt(1);
            // down_sample_path.Insert(1, new System.Numerics.Vector2(modified_first_APF_point.X, modified_first_APF_point.Y));

            int i = 1;
            List<Tuple<MathFunction.Circle, string, List<PointF>>> return_circles = new List<Tuple<MathFunction.Circle, string, List<PointF>>>();
            while (i < down_sample_path.Count - 1)
            {
                PointF previous_point = new PointF(down_sample_path[i - 1].X, down_sample_path[i - 1].Y);
                PointF now_point = new PointF(down_sample_path[i].X, down_sample_path[i].Y);
                PointF next_point = new PointF(down_sample_path[i + 1].X, down_sample_path[i + 1].Y);

                System.Numerics.Vector2 first_second_vec = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(x: now_point.X - previous_point.X, y: now_point.Y - previous_point.Y));
                System.Numerics.Vector2 second_third_vec = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(x: next_point.X - now_point.X, y: next_point.Y - now_point.Y));

                float vec_dot = System.Numerics.Vector2.Dot(System.Numerics.Vector2.Negate(first_second_vec), second_third_vec);

                if (first_second_vec.Equals(second_third_vec) != true && vec_dot <= 0)
                {
                    return_circles.Add(Retun_Circle(previous_point, now_point, next_point, first_second_vec, second_third_vec));

                    if (return_circles.Count == 1)
                    {

                        string turn_side;
                        System.Numerics.Vector2 first_normal_vec;
                        int first_turn_side = MathFunction.SideOfVector(startPos_point, heading_vec_second_point, return_circles[0].Item3[1]);

                        // right
                        if (first_turn_side == -1)
                        {
                            first_normal_vec = new System.Numerics.Vector2(x: heading_vec.Y, y: -heading_vec.X);
                            turn_side = "R";
                        }
                        // left
                        else
                        {
                            first_normal_vec = new System.Numerics.Vector2(x: -heading_vec.Y, y: heading_vec.X);
                            turn_side = "L";
                        }

                        System.Drawing.PointF first_return_center = new System.Drawing.PointF(x: startPos_point.X + 7.225f * first_normal_vec.X, startPos_point.Y + 7.225f * first_normal_vec.Y);
                        MathFunction.Circle first_return_circle = new MathFunction.Circle(first_return_center, 7.225f);

                        if (turn_side != return_circles[0].Item2)
                        {
                            float center_dist = (float)MathFunction.Distance(first_return_center, return_circles[0].Item1.center);
                            if (center_dist < 14.45f)
                            {
                                down_sample_path.RemoveAt(i);
                                return_circles = new List<Tuple<MathFunction.Circle, string, List<PointF>>>();
                                i = 1;
                                continue;
                            }
                            else
                            {
                                // 則尋求內公切線，會有兩條內公切線
                                (MathFunction.Line l1, MathFunction.Line l2) = MathFunction.InnerTagentLines(first_return_circle, return_circles[0].Item1);
                                int l1_turn_side = MathFunction.SideOfVector(first_return_center, l1.PointA, l1.PointB);

                                // float l1_cut_point_dist = (float)MathFunction.Distance(l1.PointA, startPos_point);
                                // float l2_cut_point_dist = (float)MathFunction.Distance(l2.PointA, startPos_point);

                                List<PointF> first_circle_cut_points = new List<PointF>() { startPos_point };
                                List<PointF> modified_first_APF_cut_points = new List<PointF>() { return_circles[0].Item3[1] };
                                if ((turn_side == "R" && l1_turn_side < 0) || (turn_side == "L" && l1_turn_side > 0))
                                {
                                    first_circle_cut_points.Add(l1.PointA);
                                    modified_first_APF_cut_points.Insert(0, l1.PointB);
                                }
                                else
                                {
                                    first_circle_cut_points.Add(l2.PointA);
                                    modified_first_APF_cut_points.Insert(0, l2.PointB);
                                }

                                Tuple<MathFunction.Circle, string, List<PointF>> first_circle = new Tuple<MathFunction.Circle, string, List<PointF>>(first_return_circle, turn_side, first_circle_cut_points);
                                Tuple<MathFunction.Circle, string, List<PointF>> first_APF_circle = new Tuple<MathFunction.Circle, string, List<PointF>>(return_circles[0].Item1, return_circles[0].Item2, modified_first_APF_cut_points);
                                return_circles.RemoveAt(0);
                                return_circles.Add(first_circle);
                                return_circles.Add(first_APF_circle);
                            }
                        }
                        else
                        {
                            float in2_center_dist = (float)MathFunction.Distance(first_return_center, return_circles[0].Item3[0]);
                            float out2_center_dist = (float)MathFunction.Distance(first_return_center, return_circles[0].Item3[1]);
                            if (in2_center_dist < 7.225f || out2_center_dist < 7.225f)
                            {
                                down_sample_path.RemoveAt(i);
                                return_circles = new List<Tuple<MathFunction.Circle, string, List<PointF>>>();
                                i = 1;
                                continue;
                            }
                            else
                            {
                                (MathFunction.Line l1, MathFunction.Line l2) = MathFunction.OuterTagentLines(first_return_circle, return_circles[0].Item1);
                                int l1_turn_side = MathFunction.SideOfVector(first_return_center, l1.PointA, l1.PointB);

                                // float l1_cut_point_dist = (float)MathFunction.Distance(l1.PointA, startPos_point);
                                // float l2_cut_point_dist = (float)MathFunction.Distance(l2.PointA, startPos_point);

                                List<PointF> first_circle_cut_points = new List<PointF>() { startPos_point };
                                List<PointF> modified_first_APF_cut_points = new List<PointF>() { return_circles[0].Item3[1] };
                                if ((turn_side == "R" && l1_turn_side < 0) || (turn_side == "L" && l1_turn_side > 0))
                                {
                                    first_circle_cut_points.Add(l1.PointA);
                                    modified_first_APF_cut_points.Insert(0, l1.PointB);
                                }
                                else
                                {
                                    first_circle_cut_points.Add(l2.PointA);
                                    modified_first_APF_cut_points.Insert(0, l2.PointB);
                                }

                                Tuple<MathFunction.Circle, string, List<PointF>> first_circle = new Tuple<MathFunction.Circle, string, List<PointF>>(first_return_circle, turn_side, first_circle_cut_points);
                                Tuple<MathFunction.Circle, string, List<PointF>> first_APF_circle = new Tuple<MathFunction.Circle, string, List<PointF>>(return_circles[0].Item1, return_circles[0].Item2, modified_first_APF_cut_points);
                                return_circles.RemoveAt(0);
                                return_circles.Add(first_circle);
                                return_circles.Add(first_APF_circle);
                            }

                        }

                    }

                    if (return_circles.Count >= 2)
                    {

                        float in1_out1 = (float)MathFunction.Distance(return_circles[return_circles.Count - 2].Item3[0], return_circles[return_circles.Count - 2].Item3[1]);
                        float in1_in2 = (float)MathFunction.Distance(return_circles[return_circles.Count - 2].Item3[0], return_circles[return_circles.Count - 1].Item3[0]);
                        float in1_out2 = (float)MathFunction.Distance(return_circles[return_circles.Count - 2].Item3[0], return_circles[return_circles.Count - 1].Item3[1]);
                        float out1_in2 = (float)MathFunction.Distance(return_circles[return_circles.Count - 2].Item3[1], return_circles[return_circles.Count - 1].Item3[0]);
                        float out1_out2 = (float)MathFunction.Distance(return_circles[return_circles.Count - 2].Item3[1], return_circles[return_circles.Count - 1].Item3[1]);
                        // if (in1_in2 < in1_out1 || out1_out2 < out1_in2)
                        if (!(in1_out1 < in1_in2 && out1_in2 < in1_in2 && in1_in2 < in1_out2) || out1_in2 > out1_out2)
                        {
                            down_sample_path.RemoveAt(i);
                            return_circles = new List<Tuple<MathFunction.Circle, string, List<PointF>>>();
                            i = 1;
                            continue;
                        }

                        if (return_circles[return_circles.Count - 2].Item2 != return_circles[return_circles.Count - 1].Item2)
                        {
                            float dist = (float)MathFunction.Distance(return_circles[return_circles.Count - 2].Item1.center, return_circles[return_circles.Count - 1].Item1.center);
                            if (dist < 14.45)
                            {
                                down_sample_path.RemoveAt(i);
                                return_circles = new List<Tuple<MathFunction.Circle, string, List<PointF>>>();
                                i = 1;
                                continue;
                            }
                        }

                        if (return_circles.Count >= 3)
                        {
                            float first2second = (float)MathFunction.Distance(return_circles[return_circles.Count - 3].Item1.center, return_circles[return_circles.Count - 2].Item1.center);
                            float second2third = (float)MathFunction.Distance(return_circles[return_circles.Count - 2].Item1.center, return_circles[return_circles.Count - 1].Item1.center);
                            float first2third = (float)MathFunction.Distance(return_circles[return_circles.Count - 3].Item1.center, return_circles[return_circles.Count - 1].Item1.center);
                            if (first2third < second2third || first2third < first2second)
                            {
                                down_sample_path.RemoveAt(i);
                                return_circles = new List<Tuple<MathFunction.Circle, string, List<PointF>>>();
                                i = 1;
                                continue;
                            }
                            else
                            {
                                i += 1;
                            }
                        }
                        else
                        {
                            i += 1;
                        }
                    }
                    else
                    {
                        i += 1;
                    }
                }
                else
                {
                    down_sample_path.RemoveAt(i);
                    return_circles = new List<Tuple<MathFunction.Circle, string, List<PointF>>>();
                    i = 1;
                }

            }
            // return_circles.Add(new Tuple<MathFunction.Circle, string, List<PointF>>(return_circles[return_circles.Count - 1].Item1, return_circles[return_circles.Count - 1].Item2, return_circles[return_circles.Count - 1].Item3));

            return return_circles;

        }

        public static (List<Tuple<MathFunction.Circle, string, List<PointF>>>, bool) IAPF_returnCircle(List<Tuple<System.Numerics.Vector2, float>> ships_pos, System.Numerics.Vector2 start, System.Numerics.Vector2 heading_vec, System.Numerics.Vector2 goal)
        {
            float[] k_att = new float[2] { 0.005f, 0.0075f};
            float step_size = 0.2f;
            int max_iters = 1000;
            float goal_threashold = 0.2f;
            float down_sample_step = 3.0f;

            for (int k_att_idx = 0; k_att_idx < k_att.Length; k_att_idx++)
            {
                Improved_APF iapf = new Improved_APF(start, goal, ships_pos, step_size, max_iters, goal_threashold, k_att[k_att_idx]);
                List<System.Numerics.Vector2> iapf_path = iapf.path_plan();
                List<System.Numerics.Vector2> down_sample_path = new List<System.Numerics.Vector2>();
                if (iapf.is_path_plan_success)
                {
                    int step = (int)(down_sample_step / step_size);
                    int i = step;
                    while (i < iapf_path.Count - 1)
                    {
                        down_sample_path.Add(iapf_path[i]);
                        // Console.WriteLine($"({iapf_path[i].X}, {iapf_path[i].Y})");
                        i += step;
                    }
                }
                else
                {
                    Debug.Log($"引力參數={k_att[k_att_idx]}無法規劃APF");
                    continue;
                }

                System.Drawing.PointF return_center = new System.Drawing.PointF(0.0f, 0.0f);

                System.Drawing.PointF startPos_point = new System.Drawing.PointF(start.X, start.Y);

                List<Tuple<MathFunction.Circle, string, List<PointF>>> final_circles = Executable_Circle_(down_sample_path, startPos_point, heading_vec);

                return (final_circles, iapf.is_path_plan_success);
            }

            return (new List<Tuple<MathFunction.Circle, string, List<PointF>>>(), false);

        }

    }

}