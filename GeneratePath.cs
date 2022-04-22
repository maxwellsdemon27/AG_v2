using System;
using System.Linq;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DubinsPathsTutorial
{
    public class GeneratePath : MonoBehaviour
    {
        public static (List<Tuple<MathFunction.Circle, char>> avoid_path, int push_circle_Index) GeneratePathFunc(System.Numerics.Vector3 startPos, float startHeading, List<System.Numerics.Vector3> DetectedShips, List<Tuple<System.Numerics.Vector3, char>> InitialDiamondCircle)
        {

            //To generate paths we need the position and rotation (heading) of the cars
            // 飛彈當前位置
            // System.Numerics.Vector3 startPos = new System.Numerics.Vector3(x:53.0216592737006f, y:0.0f, z:9.971033714445f);
            // System.Numerics.Vector3 startPos = new System.Numerics.Vector3(x:2.6077564927563f, y:0.0f, z:-42.3286265103765f);
            // System.Numerics.Vector3 startPos = new System.Numerics.Vector3(x:4.3175286618571f, y:0.0f, z:-41.1824537418076f);
            // System.Numerics.Vector3 startPos = new System.Numerics.Vector3(x:11.4794318215552f, y:0.0f, z:-36.3813588818469f);

            // 飛彈當前飛行角度 上(0度)、右(90度)、左(-90度)、下(+-180度)，轉弧度
            // float startHeading = (180f-123.8365476383409f) * (Mathf.PI * 2) / 360;
            // float startHeading = -45 * (Mathf.PI * 2) / 360;

            // 最終菱形搜索座標
            // System.Numerics.Vector3 goalPos = new System.Numerics.Vector3(x:57.8838464940728f, y:0.0f, z:5.1088464940728f);
            // // 最終目標座標的左迴轉圓座標
            // System.Numerics.Vector3 leftsideReturnPos = new System.Numerics.Vector3(x:52.775f, y:0.0f, z:0.0f);
            // // 最終目標座標的右迴轉圓座標
            // System.Numerics.Vector3 rightsideReturnPos = new System.Numerics.Vector3(x:62.9926929881456f, y:0.0f, z:10.2176929881456f);
            // List<(PointF center, PointF cutpoint, char direction)> NewgoalPos = new List<(PointF center, PointF cutpoint, char direction)>();
            // NewgoalPos.Add((new PointF(leftsideReturnPos.X, leftsideReturnPos.Z), 
            //                 new PointF(goalPos.X, goalPos.Z),
            //                  'L'));
            // NewgoalPos.Add((new PointF(rightsideReturnPos.X, rightsideReturnPos.Z), 
            //                 new PointF(goalPos.X, goalPos.Z),
            //                  'R'));

            // 目標的航行角度 上(0度)、右(90度)、左(-90度)、下(+-180度)，轉弧度
            // float goalHeading = -45 * (Mathf.PI * 2) / 360;
            // float goalHeading = -135 * (Mathf.PI * 2) / 360;

            // 偵查到的護衛艦座標
            // List<System.Numerics.Vector3> DetectedShips = new List<System.Numerics.Vector3>();
            // DetectedShips.Add(new System.Numerics.Vector3(x:29.6229855761661f, y:0.0f, z:25.3496574889024f));
            // DetectedShips.Add(new System.Numerics.Vector3(x:18.2988039075536f, y:0.0f, z:-10.5567215163064f));
            // DetectedShips.Add(new System.Numerics.Vector3(x:23.0008833484754f, y:0.0f, z:-23.1421631299995f));
            // DetectedShips.Add(new System.Numerics.Vector3(x:24.7106555175762f, y:0.0f, z:-21.9959903614306f));
            // DetectedShips.Add(new System.Numerics.Vector3(x:82.7536255633538f, y:0.0f, z:-22.1092239774434f));
            // DetectedShips.Add(new System.Numerics.Vector3(x:12.2624794863495f, y:0.0f, z:-0.1448869746944f));
            // DetectedShips.Add(new System.Numerics.Vector3(x:36.6852542347757f, y:0.0f, z:-24.1884709354582f));
            // DetectedShips.Add(new System.Numerics.Vector3(x:43.8774553733246f, y:0.0f, z:35.3865301042543f));
            // DetectedShips.Add(new System.Numerics.Vector3(x:2.6140094121851f, y:0.0f, z:22.087907537454f));

            // List<Tuple<System.Numerics.Vector3, char>> InitialDiamondCircle = new List<Tuple<System.Numerics.Vector3, char>>();
            // InitialDiamondCircle.Add(new Tuple<System.Numerics.Vector3, char>(new System.Numerics.Vector3(x:0.0f, y:0.0f, z:-52.775f), 'R'));
            // InitialDiamondCircle.Add(new Tuple<System.Numerics.Vector3, char>(new System.Numerics.Vector3(x:52.775f, y:0.0f, z:0.0f), 'L'));
            // InitialDiamondCircle.Add(new Tuple<System.Numerics.Vector3, char>(new System.Numerics.Vector3(x:0.0f, y:0.0f, z:52.775f), 'L'));
            // InitialDiamondCircle.Add(new Tuple<System.Numerics.Vector3, char>(new System.Numerics.Vector3(x:-52.775f, y:0.0f, z:0.0f), 'L'));
            // InitialDiamondCircle.Add(new Tuple<System.Numerics.Vector3, char>(new System.Numerics.Vector3(x:0.0f, y:0.0f, z:-52.775f), 'L'));
            // InitialDiamondCircle.Add(new Tuple<System.Numerics.Vector3, char>(new System.Numerics.Vector3(x:-7.225f, y:0.0f, z:0.0f), 'L'));


            // 回傳新的左右迴轉圓，資料結構為(圓心、切點、迴轉方向)，[0]為左迴轉、[1]為右回轉
            List<(PointF center, PointF cutpoint, char direction)> NewstartPos = NewStartPos(startPos, startHeading, DetectedShips);

            List<(PointF center, PointF cutpoint, char direction, float goalHeading, int push_circle_Index)> NewgoalPos = GetNewTarget.NewGoalPos(InitialDiamondCircle, DetectedShips);
            
            Stopwatch sw = new Stopwatch();
            sw.Start();

            UnityEngine.Debug.Log($"計算 右邊迴轉圓 至 左邊目標圓 的路徑!");
            (List<List<Tuple<MathFunction.Circle, char>>> right_left, List<float> RL_dist) = FinalDubinPath(NewstartPos[1], NewgoalPos[0], DetectedShips, startHeading, NewgoalPos[0].goalHeading);
            UnityEngine.Debug.Log($"計算 左邊迴轉圓 至 左邊目標圓 的路徑!");
            (List<List<Tuple<MathFunction.Circle, char>>> left_left, List<float> LL_dist) = FinalDubinPath(NewstartPos[0], NewgoalPos[0], DetectedShips, startHeading, NewgoalPos[0].goalHeading);
            UnityEngine.Debug.Log($"計算 右邊迴轉圓 至 右邊目標圓 的路徑!");
            (List<List<Tuple<MathFunction.Circle, char>>> right_right, List<float> RR_dist) = FinalDubinPath(NewstartPos[1], NewgoalPos[1], DetectedShips, startHeading, NewgoalPos[1].goalHeading);
            UnityEngine.Debug.Log($"計算 左邊迴轉圓 至 右邊目標圓 的路徑!");
            (List<List<Tuple<MathFunction.Circle, char>>> left_right, List<float> LR_dist) = FinalDubinPath(NewstartPos[0], NewgoalPos[1], DetectedShips, startHeading, NewgoalPos[1].goalHeading);


            //將所有路徑結果與距離依序串接
            List<List<Tuple<MathFunction.Circle, char>>> all_avoidance_path = new List<List<Tuple<MathFunction.Circle, char>>>();
            List<float> all_dist_of_paths = new List<float>();
            all_avoidance_path.AddRange(right_left);
            all_dist_of_paths.AddRange(RL_dist);

            all_avoidance_path.AddRange(left_left);
            all_dist_of_paths.AddRange(LL_dist);

            all_avoidance_path.AddRange(right_right);
            all_dist_of_paths.AddRange(RR_dist);

            all_avoidance_path.AddRange(left_right);
            all_dist_of_paths.AddRange(LR_dist);

            // 尋求最短路徑的索引值，以利取得對應路徑
            List<Tuple<MathFunction.Circle, char>> final_path = all_avoidance_path[all_dist_of_paths.IndexOf(all_dist_of_paths.Min())];

            sw.Stop();
            TimeSpan ts2 = sw.Elapsed;
            // Console.WriteLine("總共花費{0}ms. \r\n", ts2.TotalMilliseconds);  

            for (int i = 0; i < final_path.Count(); i++)
            {
                UnityEngine.Debug.Log($"避障圓{i}, 轉向 = {final_path[i].Item2}, 圓心 = {final_path[i].Item1.center}\r");
                // Console.WriteLine($"避障圓{i}, 轉向 = {final_path[i].Item2}, 圓心 = {final_path[i].Item1.center}\r");
            }

            // 更新目標圓為左圓，則要回傳左圓要推到第幾個目標圓後(index)
            if (all_dist_of_paths.IndexOf(all_dist_of_paths.Min()) == 0 || all_dist_of_paths.IndexOf(all_dist_of_paths.Min()) == 1)
            {
                return (final_path, NewgoalPos[0].push_circle_Index);
            }
            // 更新目標圓為右圓，則要回傳右圓要推到第幾個目標圓後(index)
            else
            {
                return (final_path, NewgoalPos[1].push_circle_Index);
            }

        }

        public static List<(PointF, PointF, char)> NewStartPos(System.Numerics.Vector3 startPos, float startHeading, List<System.Numerics.Vector3> DetectedShips)
        {
            float return_radius = 7.225f;
            float threaten_radius = 28.0f;

            // 將Unity座標軸：北(0)、西(-90)、東(90)、南(+-180)，轉換為標準座標軸：東(0)、北(90)、西(180)、南(270)
            float ToOrgAngleAxis = -startHeading + (Mathf.PI / 2);
            // 當前航行角度轉換成單位向量
            System.Numerics.Vector2 HeadingVec = MathFunction.GetVector(ToOrgAngleAxis, 1);

            // 航行向量的左邊法向量
            System.Numerics.Vector2 LeftVec = new System.Numerics.Vector2(x: -HeadingVec.Y, y: HeadingVec.X);
            // 航行向量的右邊法向量
            System.Numerics.Vector2 RightVec = new System.Numerics.Vector2(x: HeadingVec.Y, y: -HeadingVec.X);

            // 飛彈當強位置的左迴轉圓圓心
            PointF LeftReturnCenter = new PointF(startPos.X + return_radius * LeftVec.X,
                                                startPos.Z + return_radius * LeftVec.Y);
            // 飛彈當強位置的右迴轉圓圓心
            PointF RightReturnCenter = new PointF(startPos.X + return_radius * RightVec.X,
                                                startPos.Z + return_radius * RightVec.Y);

            // Produce new left return circle, it is same as original one at the begining.
            PointF NewLeftReturnCircle = new PointF(LeftReturnCenter.X, LeftReturnCenter.Y);
            for (int i = 0; i < DetectedShips.Count; i++)
            {

                PointF detectedship = new PointF(x: DetectedShips[i].X, y: DetectedShips[i].Z);
                // 迴轉圓圓心至當前護衛艦距離
                float ReturnToShip = (float)MathFunction.Distance(NewLeftReturnCircle, detectedship);

                if (Math.Abs(ReturnToShip - 35.225f) <= 0.00001)
                {
                    ReturnToShip = 35.225f;
                }

                // 若迴轉圓與護衛艦威脅圓重疊
                if (ReturnToShip < 35.225f)
                {
                    //線段起點為當前迴轉圓位置
                    //線段終點為當前迴轉位置-100倍的相反航行方向
                    PointF lineEnd = new PointF(NewLeftReturnCircle.X - 100 * HeadingVec.X,
                                                NewLeftReturnCircle.Y - 100 * HeadingVec.Y);

                    // 線段與"以護衛艦為圓心，半徑為28+7.225的圓"，所產生之交點，這邊使用距離護衛艦圓心較近的交點最為新回轉圓
                    // NewLeftReturnCircle = MathFunction.ClosestIntersection(detectedship.X, detectedship.Y, threaten_radius+return_radius, NewLeftReturnCircle, lineEnd);

                    PointF intersection1;
                    PointF intersection2;
                    int intersections = MathFunction.FindLineCircleIntersections(detectedship.X, detectedship.Y, threaten_radius + return_radius, NewLeftReturnCircle, lineEnd, out intersection1, out intersection2);

                    System.Numerics.Vector2 ori_center_intersect = new System.Numerics.Vector2(intersection1.X - NewLeftReturnCircle.X, intersection1.Y - NewLeftReturnCircle.Y);

                    if (System.Numerics.Vector2.Dot(HeadingVec, ori_center_intersect) < 0)
                    {
                        NewLeftReturnCircle = intersection1;
                    }
                    else
                    {
                        NewLeftReturnCircle = intersection2;
                    }
                    i = -1;
                }
            }
            // 新左迴轉圓與原航行方向的切點
            PointF NewLeftReturnCutPoint = new PointF(NewLeftReturnCircle.X + return_radius * RightVec.X,
                                                    NewLeftReturnCircle.Y + return_radius * RightVec.Y);

            // Produce new right return circle
            PointF NewRightReturnCircle = new PointF(RightReturnCenter.X, RightReturnCenter.Y);
            for (int i = 0; i < DetectedShips.Count; i++)
            {
                PointF detectedship = new PointF(x: DetectedShips[i].X, y: DetectedShips[i].Z);
                float ReturnToShip = (float)MathFunction.Distance(NewRightReturnCircle, detectedship);
                
                if (Math.Abs(ReturnToShip - 35.225f) <= 0.00001)
                {
                    ReturnToShip = 35.225f;
                }
                
                if (ReturnToShip < 35.225f)
                {
                    PointF lineEnd = new PointF(NewRightReturnCircle.X - 100 * HeadingVec.X,
                                                NewRightReturnCircle.Y - 100 * HeadingVec.Y);

                    // NewRightReturnCircle = MathFunction.ClosestIntersection(detectedship.X, detectedship.Y, threaten_radius+return_radius, NewRightReturnCircle, lineEnd);

                    PointF intersection1;
                    PointF intersection2;
                    int intersections = MathFunction.FindLineCircleIntersections(detectedship.X, detectedship.Y, threaten_radius + return_radius, NewRightReturnCircle, lineEnd, out intersection1, out intersection2);

                    System.Numerics.Vector2 ori_center_intersect = new System.Numerics.Vector2(intersection1.X - NewRightReturnCircle.X, intersection1.Y - NewRightReturnCircle.Y);

                    if (System.Numerics.Vector2.Dot(HeadingVec, ori_center_intersect) < 0)
                    {
                        NewRightReturnCircle = intersection1;
                    }
                    else
                    {
                        NewRightReturnCircle = intersection2;
                    }
                    i = -1;
                }
            }
            // 新右迴轉圓與原航行方向的切點
            PointF NewRightReturnCutPoint = new PointF(NewRightReturnCircle.X + return_radius * LeftVec.X,
                                                    NewRightReturnCircle.Y + return_radius * LeftVec.Y);


            List<(PointF center, PointF cutpoint, char direction)> NewstartPos = new List<(PointF center, PointF cutpoint, char direction)>();
            NewstartPos.Add((NewLeftReturnCircle, NewLeftReturnCutPoint, 'L'));
            NewstartPos.Add((NewRightReturnCircle, NewRightReturnCutPoint, 'R'));

            return NewstartPos;

        }

        /// <summary>  
        /// 給定迴轉圓圓心、目標圓圓心、迴轉圓切點、目標圓切點，輸出RSR的Dubin曲線
        /// </summary>
        public static OneDubinsPath GetRSR_OneDubinsPath(System.Numerics.Vector3 startcenter, System.Numerics.Vector3 goalcenter, System.Numerics.Vector3 startPos, System.Numerics.Vector3 goalPos)
        {
            //Find both tangent positons
            System.Numerics.Vector3 startTangent = System.Numerics.Vector3.Zero;
            System.Numerics.Vector3 goalTangent = System.Numerics.Vector3.Zero;
            DubinsMath.LSLorRSR(startcenter, goalcenter, false, out startTangent, out goalTangent);
            //Calculate lengths
            float length1 = DubinsMath.GetArcLength(startcenter, startPos, startTangent, false);
            float length2 = (startTangent - goalTangent).Length();
            float length3 = DubinsMath.GetArcLength(goalcenter, goalTangent, goalPos, false);
            //Save the data
            OneDubinsPath pathData = new OneDubinsPath(length1, length2, length3, startTangent, goalTangent, PathType.RSR);
            //We also need this data to simplify when generating the final path
            pathData.segment2Turning = false;
            //RSR
            pathData.SetIfTurningRight(true, false, true);

            return pathData;

        }

        /// <summary>  
        /// 給定迴轉圓圓心、目標圓圓心、迴轉圓切點、目標圓切點，輸出LSL的Dubin曲線
        /// </summary>
        public static OneDubinsPath GetLSL_OneDubinsPath(System.Numerics.Vector3 startcenter, System.Numerics.Vector3 goalcenter, System.Numerics.Vector3 startPos, System.Numerics.Vector3 goalPos)
        {
            //Find both tangent positons
            System.Numerics.Vector3 startTangent = System.Numerics.Vector3.Zero;
            System.Numerics.Vector3 goalTangent = System.Numerics.Vector3.Zero;
            DubinsMath.LSLorRSR(startcenter, goalcenter, true, out startTangent, out goalTangent);
            //Calculate lengths
            float length1 = DubinsMath.GetArcLength(startcenter, startPos, startTangent, true);
            float length2 = (startTangent - goalTangent).Length();
            float length3 = DubinsMath.GetArcLength(goalcenter, goalTangent, goalPos, true);
            //Save the data
            OneDubinsPath pathData = new OneDubinsPath(length1, length2, length3, startTangent, goalTangent, PathType.LSL);
            //We also need this data to simplify when generating the final path
            pathData.segment2Turning = false;
            //LSL
            pathData.SetIfTurningRight(false, false, false);

            return pathData;

        }

        /// <summary>  
        /// 給定迴轉圓圓心、目標圓圓心、迴轉圓切點、目標圓切點，輸出RSL的Dubin曲線
        /// </summary>
        public static OneDubinsPath GetRSL_OneDubinsPath(System.Numerics.Vector3 startcenter, System.Numerics.Vector3 goalcenter, System.Numerics.Vector3 startPos, System.Numerics.Vector3 goalPos)
        {
            //Find both tangent positons
            System.Numerics.Vector3 startTangent = System.Numerics.Vector3.Zero;
            System.Numerics.Vector3 goalTangent = System.Numerics.Vector3.Zero;
            DubinsMath.RSLorLSR(startcenter, goalcenter, false, out startTangent, out goalTangent);
            //Calculate lengths
            float length1 = DubinsMath.GetArcLength(startcenter, startPos, startTangent, false);
            float length2 = (startTangent - goalTangent).Length();
            float length3 = DubinsMath.GetArcLength(goalcenter, goalTangent, goalPos, true);
            //Save the data
            OneDubinsPath pathData = new OneDubinsPath(length1, length2, length3, startTangent, goalTangent, PathType.RSL);
            //We also need this data to simplify when generating the final path
            pathData.segment2Turning = false;
            //RSL
            pathData.SetIfTurningRight(true, false, false);

            return pathData;
        }

        /// <summary>  
        /// 給定迴轉圓圓心、目標圓圓心、迴轉圓切點、目標圓切點，輸出LSR的Dubin曲線
        /// </summary>
        public static OneDubinsPath GetLSR_OneDubinsPath(System.Numerics.Vector3 startcenter, System.Numerics.Vector3 goalcenter, System.Numerics.Vector3 startPos, System.Numerics.Vector3 goalPos)
        {
            //Find both tangent positons
            System.Numerics.Vector3 startTangent = System.Numerics.Vector3.Zero;
            System.Numerics.Vector3 goalTangent = System.Numerics.Vector3.Zero;
            DubinsMath.RSLorLSR(startcenter, goalcenter, true, out startTangent, out goalTangent);
            //Calculate lengths
            float length1 = DubinsMath.GetArcLength(startcenter, startPos, startTangent, true);
            float length2 = (startTangent - goalTangent).Length();
            float length3 = DubinsMath.GetArcLength(goalcenter, goalTangent, goalPos, false);
            //Save the data
            OneDubinsPath pathData = new OneDubinsPath(length1, length2, length3, startTangent, goalTangent, PathType.LSR);
            //We also need this data to simplify when generating the final path
            pathData.segment2Turning = false;
            //LSR
            pathData.SetIfTurningRight(false, false, true);

            return pathData;
        }

        /// <summary>  
        /// 給定新起始迴轉圓、新目標圓、飛彈當前航向、飛彈目標最終航向，以及當前偵測到的所有護衛艦位置資訊，
        /// 尋求所有路徑的的迴轉圓與迴轉方向，及其對應的圓心連線總距離
        /// </summary>
        /// <param name="NewstartPos">新起始迴轉圓，資料結構為(圓心、切點、迴轉方向)，[0]為左迴轉、[1]為右回轉</param>
        /// <param name="NewgoalPos">新目標圓，資料結構為(圓心、切點、迴轉方向)，[0]為左迴轉、[1]為右回轉</param>
        /// <param name="DetectedShips">偵測到的護衛艦</param>
        /// <param name="startHeading">飛彈當前航向</param>
        /// <param name="goalHeading">飛彈目標最終航向</param>
        /// <returns>最短路徑的所有迴轉圓與迴轉方向</returns>
        public static (List<List<Tuple<MathFunction.Circle, char>>>, List<float>) FinalDubinPath((PointF, PointF, char) NewstartPos, (PointF, PointF, char, float, int) NewgoalPos,
                                                List<System.Numerics.Vector3> DetectedShips, float startHeading, float goalHeading)
        {

            //Objects
            DubinsGeneratePaths dubinsPathGenerator = new DubinsGeneratePaths();

            // Item1 is center
            System.Numerics.Vector3 startcenter = new System.Numerics.Vector3(x: NewstartPos.Item1.X, y: 0.0f, z: NewstartPos.Item1.Y);
            System.Numerics.Vector3 goalcenter = new System.Numerics.Vector3(x: NewgoalPos.Item1.X, y: 0.0f, z: NewgoalPos.Item1.Y);

            // Item2 is cutpoint
            System.Numerics.Vector3 startPos = new System.Numerics.Vector3(x: NewstartPos.Item2.X, y: 0.0f, z: NewstartPos.Item2.Y);
            System.Numerics.Vector3 goalPos = new System.Numerics.Vector3(x: NewgoalPos.Item2.X, y: 0.0f, z: NewgoalPos.Item2.Y);

            //Get all valid Dubins paths
            // List<OneDubinsPath> pathDataList = dubinsPathGenerator.GetAllDubinsPaths(
            //     startPos, 
            //     startHeading,
            //     goalPos,
            //     goalHeading);


            // Position the left and right circles
            // System.Numerics.Vector3 goalLeft = dubinsPathGenerator.goalLeftCircle;
            // System.Numerics.Vector3 goalRight = dubinsPathGenerator.goalRightCircle;
            // System.Numerics.Vector3 startLeft = dubinsPathGenerator.startLeftCircle;
            // System.Numerics.Vector3 startRight = dubinsPathGenerator.startRightCircle;

            // Choose the target circle
            // Remove the dubin path witch is wrong direction of start pos
            // for(int i = pathDataList.Count-1; i >= 0; i--)
            // {
            //     if (pathDataList[i].pathType.ToString()[0] != NewstartPos.Item3 || 
            //         pathDataList[i].pathType.ToString()[2] != NewgoalPos.Item3)
            //     {
            //         pathDataList.RemoveAt(i);
            //     }

            // }

            // 根據迴轉圓與目標圓的方向，製造出對應的dubin曲線路徑
            List<OneDubinsPath> pathDataList = new List<OneDubinsPath>();
            OneDubinsPath pathData;
            if (NewstartPos.Item3 == NewgoalPos.Item3)
            {
                //RSR and LSL is only working if the circles don't have the same position
                if (startcenter.X != goalcenter.X && startcenter.Z != goalcenter.Z)
                {
                    // RSR
                    if (NewstartPos.Item3 == 'R')
                    {
                        pathData = GetRSR_OneDubinsPath(startcenter, goalcenter, startPos, goalPos);
                    }
                    // LSL
                    else
                    {
                        pathData = GetLSL_OneDubinsPath(startcenter, goalcenter, startPos, goalPos);
                    }
                    //  Add the path to the collection of all paths
                    pathDataList.Add(pathData);
                }
            }
            else
            {
                //RSL and LSR is only working of the circles don't intersect
                float comparisonSqr = DubinsMath.turningRadius * 2f * DubinsMath.turningRadius * 2f;
                if ((startcenter - goalcenter).LengthSquared() > comparisonSqr)
                {
                    // RSL
                    if (NewstartPos.Item3 == 'R')
                    {
                        pathData = GetRSL_OneDubinsPath(startcenter, goalcenter, startPos, goalPos);
                    }
                    // LSR
                    else
                    {
                        pathData = GetLSR_OneDubinsPath(startcenter, goalcenter, startPos, goalPos);
                    }
                    //  Add the path to the collection of all paths
                    pathDataList.Add(pathData);
                }
            }

            // 計算路徑上每個迴轉圓圓心的連線距離
            List<List<Tuple<MathFunction.Circle, char>>> list_all_return_circles = MathFunction.AllReturnCircle(startcenter, goalcenter, pathDataList[0], DetectedShips);
            List<float> path_dist = new List<float>();
            
            if (list_all_return_circles == null)
            {
                path_dist.Add(float.MaxValue);
                return (null, path_dist);
            }
            else
            {
                for (int i = 0; i < list_all_return_circles.Count; i++)
                {
                    float dist = (float)MathFunction.Distance(NewstartPos.Item1, list_all_return_circles[i][0].Item1.center);
                    for (int j = 0; j < list_all_return_circles[i].Count - 1; j++)
                    {
                        dist += (float)MathFunction.Distance(list_all_return_circles[i][j].Item1.center, list_all_return_circles[i][j + 1].Item1.center);
                    }
                    path_dist.Add(dist);
                }

                return (list_all_return_circles, path_dist);

            }

        }

    }
}
