using System;
using System.Drawing;
using System.Numerics;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace DubinsPathsTutorial
{
    public static class GetNewTarget
    {
        public static List<(PointF center, PointF cutpoint, char direction, float goalHeading, int push_circle_Index)> NewGoalPos(List<Tuple<System.Numerics.Vector3, char>> InitialDiamondCircle, List<System.Numerics.Vector3> DetectedShips, float return_radius=7.225f)
        {
            
            
            List<(PointF center, PointF cutpoint, char direction, float goalHeading, int push_circle_Index)> NewgoalPos = new List<(PointF center, PointF cutpoint, char direction, float goalHeading, int push_circle_Index)>();

            (PointF center, PointF cutpoint, char direction, float goalHeading, int push_circle_Index) ori_left_return = GetFinalGoalCircle(InitialDiamondCircle, DetectedShips, return_radius, turn_side:'L');
            (PointF center, PointF cutpoint, char direction, float goalHeading, int push_circle_Index) ori_right_return = GetFinalGoalCircle(InitialDiamondCircle, DetectedShips, return_radius, turn_side:'R');

            NewgoalPos.Add(ori_left_return);
            NewgoalPos.Add(ori_right_return);

            return NewgoalPos;
        }

        public static PointF PushNewGoal(List<System.Numerics.Vector3> DetectedShips, MathFunction.Line tangent_line, PointF ori_center, float return_radius=7.225f)
        {
            System.Numerics.Vector2 target_cruise_vec = new System.Numerics.Vector2(x: tangent_line.PointB.X - tangent_line.PointA.X, y: tangent_line.PointB.Y - tangent_line.PointA.Y);
            PointF second_point = new PointF(x:ori_center.X + target_cruise_vec.X, y: ori_center.Y + target_cruise_vec.Y);
            for(int i = 0; i<DetectedShips.Count; i++)
            {
                PointF ship_pos = new PointF(x:DetectedShips[i].X, y:DetectedShips[i].Z);
                double goal_ship_dist = MathFunction.Distance(ori_center, ship_pos);
                if (goal_ship_dist < 28 + return_radius)
                {
                    PointF intersection1;
                    PointF intersection2;
                    int intersections = MathFunction.FindLineCircleIntersections(ship_pos.X, ship_pos.Y, 28 + return_radius, ori_center, second_point, out intersection1, out intersection2);

                    System.Numerics.Vector2 ori_center_intersect = new System.Numerics.Vector2(intersection1.X - ori_center.X, intersection1.Y - ori_center.Y);
                    if (System.Numerics.Vector2.Dot(target_cruise_vec, ori_center_intersect) > 0)
                    {
                        ori_center = intersection1;
                    }
                    else
                    {
                        ori_center = intersection2;
                    }
                    i = 0;
                }
                
            }
            return ori_center;
        }

        public static (PointF center, PointF cutpoint, char direction, float goalHeading, int push_circle_Index) GetFinalGoalCircle(List<Tuple<System.Numerics.Vector3, char>> InitialDiamondCircle, List<System.Numerics.Vector3> DetectedShips, float return_radius=7.225f, char turn_side = 'L')
        {
            
            int push_circle_Index = 0;
            while(true)
            {
                string dubin_type = InitialDiamondCircle[push_circle_Index].Item2 + "s" + InitialDiamondCircle[push_circle_Index+1].Item2;
                MathFunction.Circle current_target_circle = new MathFunction.Circle(new PointF(x:InitialDiamondCircle[push_circle_Index].Item1.X, y:InitialDiamondCircle[push_circle_Index].Item1.Z), return_radius);
                MathFunction.Circle next_target_circle = new MathFunction.Circle(new PointF(x:InitialDiamondCircle[push_circle_Index+1].Item1.X, y:InitialDiamondCircle[push_circle_Index+1].Item1.Z), return_radius);
                MathFunction.Line tangent_line = MathFunction.ChooseTangentLine(current_target_circle, next_target_circle, dubin_type);

                System.Numerics.Vector2 target_cruise_vec = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(x:tangent_line.PointB.X - tangent_line.PointA.X,
                                                                        y:tangent_line.PointB.Y - tangent_line.PointA.Y));
                // System.Numerics.Vector2 target_cruise_vec = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(x:InitialDiamondCircle[push_circle_Index+1].Item1.X-InitialDiamondCircle[push_circle_Index].Item1.X,
                //                                                         y:InitialDiamondCircle[push_circle_Index+1].Item1.Z-InitialDiamondCircle[push_circle_Index].Item1.Z));
                System.Numerics.Vector2 left_normal_vec = new System.Numerics.Vector2(-target_cruise_vec.Y, target_cruise_vec.X);
                System.Numerics.Vector2 right_normal_vec = new System.Numerics.Vector2(target_cruise_vec.Y, -target_cruise_vec.X);

                (PointF center, PointF cutpoint, char direction) ori_return;
                if(turn_side == 'L')
                {
                    ori_return = (new PointF(tangent_line.PointA.X + return_radius * left_normal_vec.X, 
                                            tangent_line.PointA.Y + return_radius * left_normal_vec.Y),
                                tangent_line.PointA, 'L');
                }
                else
                {
                    ori_return = (new PointF(tangent_line.PointA.X + return_radius * right_normal_vec.X, 
                                            tangent_line.PointA.Y + return_radius * right_normal_vec.Y),
                                tangent_line.PointA, 'R');

                }
                PointF left_return_end = new PointF(tangent_line.PointB.X + return_radius * left_normal_vec.X, 
                                                    tangent_line.PointB.Y + return_radius * left_normal_vec.Y);

                PointF right_return_end = new PointF(tangent_line.PointB.X + return_radius * right_normal_vec.X, 
                                            tangent_line.PointB.Y + return_radius * right_normal_vec.Y);

                PointF new_goal_center = PushNewGoal(DetectedShips, tangent_line, ori_return.center, return_radius);

                if (MathFunction.SideOfLine(new_goal_center, left_return_end, right_return_end) == MathFunction.SideOfLine(ori_return.center, left_return_end, right_return_end) || push_circle_Index == InitialDiamondCircle.Count-2)
                {
                    PointF new_cut_point;
                    if (turn_side == 'L')
                    {
                        new_cut_point = new PointF(x:new_goal_center.X + return_radius * right_normal_vec.X,
                                                        y:new_goal_center.Y + return_radius * right_normal_vec.Y);
                        
                    }
                    else
                    {
                        new_cut_point = new PointF(x:new_goal_center.X + return_radius * left_normal_vec.X,
                                                        y:new_goal_center.Y + return_radius * left_normal_vec.Y);                        
                    }
                    float heading_angle = -(180 * Mathf.Atan2(target_cruise_vec.Y, target_cruise_vec.X) / Mathf.PI - 90) * (Mathf.PI / 180);
                    // float heading_angle = -Mathf.Atan2(target_cruise_vec.Y, target_cruise_vec.X) + (Mathf.PI/2);
                    return (new_goal_center, new_cut_point, turn_side, heading_angle, push_circle_Index);
                    
                }
                else if(push_circle_Index < InitialDiamondCircle.Count)
                {
                    push_circle_Index += 1;
                }
            }
        }
    }
}