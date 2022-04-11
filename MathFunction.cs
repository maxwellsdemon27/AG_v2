using System;
using System.Linq;
using System.Drawing;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DubinsPathsTutorial
{
    public class MathFunction : MonoBehaviour
    {        
        public class Line{

            public PointF PointA;

            public PointF PointB;

            public Line(PointF PointA, PointF PointB){
                this.PointA = PointA;
                this.PointB = PointB;
            }

        }

        public class Circle{
            public PointF center;
            public float radius;

            public Circle(PointF center, float radius){
                this.center = center;
                this.radius = radius;
            }

        }

        public class AvoidanceTree{
            public Circle avoid_circle;
            public Line tangent_line;
            public List<AvoidanceTree> next_stage;

            public AvoidanceTree(Circle avoid_circle, Line tangent_line, List<AvoidanceTree> next_stage ){
                this.avoid_circle = avoid_circle;
                this.tangent_line = tangent_line;
                this.next_stage = next_stage;
            }
        }
        
        public static double Distance(PointF p1, PointF p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }

        /// <summary>
        /// 將角度轉為向量，角度系為 左0 上90 右180 下270
        /// </summary>
        /// <param name="angle">弧度</param>
        /// <param name="distance">距離</param>
        /// <returns>向量</returns>
        public static System.Numerics.Vector2 GetVector(float radian, float distance)
        {
            //計算新座標 r 就是兩者的距離
            System.Numerics.Vector2 vec = new System.Numerics.Vector2(x:(float)(distance * Math.Cos(radian)), 
                                        y:(float)(distance * Math.Sin(radian)));

            return vec;
        }
        
        
        //点到线段距离  
        public static double pointToLine(Line l1, PointF p1)
        {    
            double space = 0;    
            double a, b, c;    
            a = Distance(l1.PointA, l1.PointB);// 线段的长度    
            b = Distance(l1.PointA, p1);// (x1,y1)到点的距离    
            c = Distance(l1.PointB, p1);// (x2,y2)到点的距离    
            if (c <= 0.000001 || b <= 0.000001) {    
                space = 0;    
                return space;    
            }    
            if (a <= 0.000001) {    
                space = b;    
                return space;    
            }    
            if (c * c >= a * a + b * b) {    
                space = b;    
                return space;    
            }    
            if (b * b >= a * a + c * c) {    
                space = c;    
                return space;    
            }    
                double p = (a + b + c) / 2;// 半周长    
                double s = Math.Sqrt(p * (p - a) * (p - b) * (p - c));// 海伦公式求面积    
            space = 2 * s / a;// 返回点到线的距离（利用三角形面积公式求高）    
            return space;    
        }

        /// <summary>
        /// 判斷點在線的左邊還是右邊，兩點p1(x1,y1),p2(x2,y2),判斷點p(x,y)在線的左邊還是右邊
        /// </summary>
        /// <returns>-1:p在線的左邊; 1:p在線的右邊; 0:p點為(Nan, Nan)</returns>
        public static int SideOfLine(PointF p, PointF p1, PointF p2)
        {
            if (double.IsNaN(p.X) || double.IsNaN(p.Y)){
                return 0;
            }
            else{
                double tmpx = (p1.X - p2.X) / (p1.Y - p2.Y) * (p.Y - p2.Y) + p2.X;

                if (tmpx > p.X)//當tmpx>p.x的時候，說明點在線的左邊，小於在右邊，等於則在線上。
                    return -1;
                return 1;

            }
        }


        /// <summary>
        /// 求直线上的投影点
        /// </summary>
        /// <param name="P1">直线上的点1</param>
        /// <param name="P2">直线上的点2</param>
        /// <param name="P3">直线外的点</param>
        /// <returns></returns>
        public static PointF LinePointProjection(PointF P1,PointF P2,PointF P3)
        {
            double a1 = P2.X - P1.X;
            double b1 = P2.Y - P1.Y;
            double y1 = P1.Y;
            double x1 = P1.X;
            double y2 = P2.Y;
            double x2 = P2.X;
            double y3 = P3.Y;
            double x3 = P3.X;
            double a1a1 = Math.Pow(a1, 2.0);
            double b1b1 = Math.Pow(b1, 2.0);
            double denominator = a1a1 + b1b1;
            if (denominator == 0) return P3;
 
            double x1y2 = x1 * y2;
            double x2y1 = x2 * y1;
            double a1b1 = a1 * b1;
            double moleculey = b1b1 * y3 + a1b1 * x3 - a1 * x1y2 + a1 * x2y1;
            double moleculex = a1a1 * x3 + a1b1 * y3 - b1 * x2y1 + b1 * x1y2;
 
            return new PointF((float)(moleculex/denominator),(float)(moleculey/denominator));
        }

        /// <summary>
        /// 給定三個座標點A,B,C，判斷AC向量在AB的左邊或右邊
        /// </summary>
        /// <returns>右邊:-1;左邊:1</returns>
        public static int SideOfVector(PointF A, PointF B, PointF C)
        {
            System.Numerics.Vector2 ab = new System.Numerics.Vector2(B.X - A.X, B.Y - A.Y);
            System.Numerics.Vector2 ac = new System.Numerics.Vector2(C.X - A.X, C.Y - A.Y);

            float cross_product = ab.X * ac.Y - ac.X * ab.Y;

            // cross_product >= 0, AC向量AB的左邊, return 1
            if (cross_product >= 0) return 1;
            // cross_product < 0, AC向量AB的右邊, return -1
            else return -1;
        }

        /// <summary>
        /// 計算直線與圓最近的交點
        /// 圆, 圆心(cx, cy), 半径radius.
        /// lineStart 線段起點
        /// lineEnd 線段終點
        /// </summary>
        /// <returns>返回直線與圓最近的交點 (PointF.X, PointF.Y)</returns>
        public static PointF ClosestIntersection(float cx, float cy, float radius, PointF lineStart, PointF lineEnd)
        {
            PointF intersection1;
            PointF intersection2;
            int intersections = FindLineCircleIntersections(cx, cy, radius, lineStart, lineEnd, out intersection1, out intersection2);

            // 交於一點(相切)
            if (intersections == 1)
                return intersection1;
            // 交於兩點
            else if (intersections == 2)
            {
                double dist1 = Distance(intersection1, lineStart);
                double dist2 = Distance(intersection2, lineStart);

                if (dist1 < dist2)
                    return intersection1;
                else
                    return intersection2;
            }
            // 沒有交點
            else{
                return PointF.Empty; // no intersections at all
            }
        }

        // Find the points of intersection.
        public static int FindLineCircleIntersections(float cx, float cy, float radius, PointF point1, PointF point2, 
                                                out PointF intersection1, out PointF intersection2)
        {
            float dx, dy, A, B, C, det, t;

            dx = point2.X - point1.X;
            dy = point2.Y - point1.Y;

            A = dx * dx + dy * dy;
            B = 2 * (dx * (point1.X - cx) + dy * (point1.Y - cy));
            C = (point1.X - cx) * (point1.X - cx) + (point1.Y - cy) * (point1.Y - cy) - radius * radius;

            det = B * B - 4 * A * C;
            if ((A <= 0.0000001) || (det < 0))
            {
                // No real solutions.
                intersection1 = new PointF(float.NaN, float.NaN);
                intersection2 = new PointF(float.NaN, float.NaN);
                return 0;
            }
            else if (det == 0)
            {
                // One solution.
                t = -B / (2 * A);
                intersection1 = new PointF(point1.X + t * dx, point1.Y + t * dy);
                intersection2 = new PointF(float.NaN, float.NaN);
                return 1;
            }
            else
            {
                // Two solutions.
                t = (float)((-B + Math.Sqrt(det)) / (2 * A));
                intersection1 = new PointF(point1.X + t * dx, point1.Y + t * dy);
                t = (float)((-B - Math.Sqrt(det)) / (2 * A));
                intersection2 = new PointF(point1.X + t * dx, point1.Y + t * dy);
                return 2;
            }
        }

        /// <summary>
        /// 计算两个相离的圆的内公切线。（相交没有内公切线，只有外公切线）
        /// 圆C1, 圆心(a, b), 半径r1.
        /// 圆C2, 圆心(c, d), 半径r2.
        /// </summary>
        /// <returns>返回两条内公切线段，线段的两个端点是圆上的切点, 每條線段的順序為C1的切點，再來才是C2的切點。</returns>
        public static (Line l1, Line l2) InnerTagentLines(Circle circle1, Circle circle2)
        {
            double a = circle1.center.X;
            double b = circle1.center.Y;
            double r1 = circle1.radius;

            double c = circle2.center.X;
            double d = circle2.center.Y;
            double r2 = circle2.radius;

            var r3 = r1 + r2;
            var sigma_1 = Math.Sqrt(a * a - 2 * a * c + b * b - 2 * b * d + c * c + d * d - r3 * r3);
            var sigma_2 = (a - c) * (a * a - 2 * a * c + b * b - 2 * b * d + c * c + d * d);
            var sigma_3 = (-a * a + c * a - b * b + d * b + r3 * r3) / (a - c);
            var sigma_4 = 2 * b * b * d;

            // 计算C3切点(x3_1, y3_1), (x3_2, y3_2)
            var x3_1 = -sigma_3 - (b - d) * (a * a * b + b * c * c + b * d * d - sigma_4 - b * r3 * r3 + d * r3 * r3 + b * b * b + a * r3 * sigma_1 - c * r3 * sigma_1 - 2 * a * b * c) / sigma_2;
            var x3_2 = -sigma_3 - (b - d) * (a * a * b + b * c * c + b * d * d - sigma_4 - b * r3 * r3 + d * r3 * r3 + b * b * b - a * r3 * sigma_1 + c * r3 * sigma_1 - 2 * a * b * c) / sigma_2;

            sigma_1 = Math.Sqrt(a * a - 2 * a * c + b * b - 2 * b * d + c * c + d * d - r3 * r3);
            sigma_2 = a * a - 2 * a * c + b * b - 2 * b * d + c * c + d * d;
            sigma_3 = 2 * b * b * d;

            var y3_1 = (a * a * b + b * c * c + b * d * d - sigma_3 - b * r3 * r3 + d * r3 * r3 + b * b * b + a * r3 * sigma_1 - c * r3 * sigma_1 - 2 * a * b * c) / sigma_2;
            var y3_2 = (a * a * b + b * c * c + b * d * d - sigma_3 - b * r3 * r3 + d * r3 * r3 + b * b * b - a * r3 * sigma_1 + c * r3 * sigma_1 - 2 * a * b * c) / sigma_2;
            
            // 计算C2切点(x2_1, y2_1, x2_2, y2_2)
            var λ = r2 / r3;
            var x2_1 = λ * a + (1 - λ) * x3_1;
            var y2_1 = λ * b + (1 - λ) * y3_1;
            var x2_2 = λ * a + (1 - λ) * x3_2;
            var y2_2 = λ * b + (1 - λ) * y3_2;

            // 计算C1切点(x1_1, y1_1), （x2_1, y2_1)
            var x1_1 = x2_1 - x3_1 + c;
            var y1_1 = y2_1 - y3_1 + d;
            var x1_2 = x2_2 - x3_2 + c;
            var y1_2 = y2_2 - y3_2 + d;


            Line l1 = new Line(new PointF((float)x2_1, (float)y2_1), new PointF((float)x1_1, (float)y1_1));
            Line l2 = new Line(new PointF((float)x2_2, (float)y2_2), new PointF((float)x1_2, (float)y1_2));
            return (l1, l2);
        }
        
        /// <summary>
        /// 計算兩圓circle1與circle2的外公切線，並返回兩外公切線，每條線由兩切點組成
        /// </summary>
        /// <param name="circle1">圓1</param>
        /// <param name="circle2">圓2</param>
        /// <returns>(Line l1, Line l2)</returns>
        public static (Line l1, Line l2) OuterTagentLines(Circle circle1, Circle circle2)
        {
            //兩外公切線
            Line l1, l2;

            PointF[] CutPoints = new PointF[2];
            if (circle1.radius != circle2.radius){
                (l1, l2) = CalculateForDifferentRadius(circle1, circle2);
                return (l1, l2);
            }else{
                System.Numerics.Vector2 c1_c2_vector = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(circle2.center.X-circle1.center.X, circle2.center.Y-circle1.center.Y));
                System.Numerics.Vector2 left_normal_vector = new System.Numerics.Vector2(-c1_c2_vector.Y, c1_c2_vector.X);
                System.Numerics.Vector2 right_normal_vector = new System.Numerics.Vector2(c1_c2_vector.Y, -c1_c2_vector.X);

                PointF left_point_1 = new PointF(circle1.center.X + circle1.radius * left_normal_vector.X,
                                                circle1.center.Y + circle1.radius * left_normal_vector.Y);

                PointF left_point_2 = new PointF(circle2.center.X + circle2.radius * left_normal_vector.X,
                                                circle2.center.Y + circle2.radius * left_normal_vector.Y);

                PointF right_point_1 = new PointF(circle1.center.X + circle1.radius * right_normal_vector.X,
                                                circle1.center.Y + circle1.radius * right_normal_vector.Y);

                PointF right_point_2 = new PointF(circle2.center.X + circle2.radius * right_normal_vector.X,
                                                circle2.center.Y + circle2.radius * right_normal_vector.Y);               

                
                l1 = new Line(new PointF((float)left_point_1.X, (float)left_point_1.Y), new PointF((float)left_point_2.X, (float)left_point_2.Y));
                l2 = new Line(new PointF((float)right_point_1.X, (float)right_point_1.Y), new PointF((float)right_point_2.X, (float)right_point_2.Y));

                return (l1, l2);
            }
        }

        /// <summary>
        /// 在半径不相等的情况下,计算 circle1 和 circle2 两圆的外公切线
        /// </summary>
        /// <param name="circle1">圓1</param>
        /// <param name="circle2">圓2</param>
        public static (Line l1, Line l2) CalculateForDifferentRadius(Circle circle1, Circle circle2)
        {
            //令circle1，circle2的外公切线交点P
            //circle1的圆心为O1,circle2的圆心为O2         
            //切线于circle1的两个焦点分别记作A1,A2
            //切线于circle2的两个焦点分别记作B1,B2
            //过点 B1 引 O1O2 的垂线，垂足为 M
            // O1O2 的斜率为k
            // 切点
            PointF[] CutPoints = new PointF[2];
            // 切線
            Line l1 ;
            Line l2 ;

            float deltaX = circle2.center.X - circle1.center.X;
            float deltaY = circle2.center.Y - circle1.center.Y;
            // O1 与 O2 距离
            float distance = 0;
            // P 与 O2 距离
            float lengthA;
            // P 与 B1 或 B2点的距离
            float lengthB;
            // B1 到 O1O2的距离
            float lengthC;
            // O2 到 M 的距离
            float lengthD;
            //用于记录 M 点的坐标
            PointF M = new PointF();
            //  O1O2 直线方程的斜率k
            float k;
            
            //  O1O2 直线方程中的常数
            float b;
            distance = (float)(Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2)));
            lengthA = distance * circle1.radius / (circle2.radius - circle1.radius);
            lengthB = (float)(Math.Sqrt(Math.Pow(lengthA, 2) - Math.Pow(circle1.radius, 2)));
            lengthC = lengthB * circle1.radius / lengthA;
            lengthD = circle1.radius * circle1.radius / lengthA;
            M.X = circle1.center.X + lengthD * -deltaX / distance;
            M.Y = circle1.center.Y + lengthD * -deltaY / distance;
            k = (circle1.center.Y - circle2.center.Y) / (circle1.center.X - circle2.center.X);
            b = circle1.center.Y - k * circle1.center.X;

            float Y1_1 = 0;
            float X1_1 = 0;
            Y1_1 = (float)((k * M.X + M.Y * k * k + b + Math.Abs(lengthC) * Math.Sqrt(k * k + 1)) / (k * k + 1));
            X1_1 = M.X - (Y1_1 - M.Y) * k;
            CutPoints[0].X = X1_1;
            CutPoints[0].Y = Y1_1;

            // normalized vector from circle2 to b1
            System.Numerics.Vector2 circle1_b1 = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(X1_1 - circle1.center.X, Y1_1 - circle1.center.Y));
            float X1_2 = circle2.center.X + circle2.radius * circle1_b1.X;
            float Y1_2 = circle2.center.Y + circle2.radius * circle1_b1.Y;

            l1 = new Line(new PointF((float)X1_1, (float)Y1_1), new PointF((float)X1_2, (float)Y1_2));

            float Y2_1 = 0;
            float X2_1 = 0;
            Y2_1 = (float)((k * M.X + M.Y * k * k + b - Math.Abs(lengthC) * Math.Sqrt(k * k + 1)) / (k * k + 1));
            X2_1 = M.X - (Y2_1 - M.Y) * k;
            CutPoints[1].X = X2_1;
            CutPoints[1].Y = Y2_1;

            // normalized vector from circle2 to b2
            System.Numerics.Vector2 circle1_b2 = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(X2_1 - circle1.center.X, Y2_1 - circle1.center.Y));
            float X2_2 = circle2.center.X + circle2.radius * circle1_b2.X;
            float Y2_2 = circle2.center.Y + circle2.radius * circle1_b2.Y;
            
            l2 = new Line(new PointF((float)X2_1, (float)Y2_1), new PointF((float)X2_2, (float)Y2_2));

            return (l1, l2);
        } 
    
        /// <summary>
        /// 計算兩條直線的交點
        /// </summary>
        /// <param name="lineFirstStar">L1的點1坐標</param>
        /// <param name="lineFirstEnd">L1的點2坐標</param>
        /// <param name="lineSecondStar">L2的點1坐標</param>
        /// <param name="lineSecondEnd">L2的點2坐標</param>
        /// <returns>PointF交點坐標</returns>
        public static PointF GetIntersection(PointF lineFirstStar, PointF lineFirstEnd, PointF lineSecondStar, PointF lineSecondEnd)
        {
            /*
             * L1，L2都存在斜率的情況：
             * 直線方程L1: ( y - y1 ) / ( y2 - y1 ) = ( x - x1 ) / ( x2 - x1 ) 
             * => y = [ ( y2 - y1 ) / ( x2 - x1 ) ]( x - x1 ) + y1
             * 令 a = ( y2 - y1 ) / ( x2 - x1 )
             * 有 y = a * x - a * x1 + y1   .........1
             * 直線方程L2: ( y - y3 ) / ( y4 - y3 ) = ( x - x3 ) / ( x4 - x3 )
             * 令 b = ( y4 - y3 ) / ( x4 - x3 )
             * 有 y = b * x - b * x3 + y3 ..........2
             * 
             * 如果 a = b，則兩直線平等，否則， 聯解方程 1,2，得:
             * x = ( a * x1 - b * x3 - y1 + y3 ) / ( a - b )
             * y = a * x - a * x1 + y1
             * 
             * L1存在斜率, L2平行Y軸的情況：
             * x = x3
             * y = a * x3 - a * x1 + y1
             * 
             * L1 平行Y軸，L2存在斜率的情況：
             * x = x1
             * y = b * x - b * x3 + y3
             * 
             * L1與L2都平行Y軸的情況：
             * 如果 x1 = x3，那麼L1與L2重合，否則平等
             * 
            */
            float a = 0, b = 0;
            int state = 0;
            if (lineFirstStar.X != lineFirstEnd.X)
            {
                a = (lineFirstEnd.Y - lineFirstStar.Y) / (lineFirstEnd.X - lineFirstStar.X);
                state |= 1;
            }
            if (lineSecondStar.X != lineSecondEnd.X)
            {
                b = (lineSecondEnd.Y - lineSecondStar.Y) / (lineSecondEnd.X - lineSecondStar.X);
                state |= 2;
            }
            switch (state)
            {
                case 0: //L1與L2都平行Y軸
                    {
                        if (lineFirstStar.X == lineSecondStar.X)
                        {
                            //throw new Exception("兩條直線互相重合，且平行於Y軸，無法計算交點。");
                            return new PointF(float.NaN, float.NaN);
                        }
                        else
                        {
                            //throw new Exception("兩條直線互相平行，且平行於Y軸，無法計算交點。");
                            return new PointF(float.NaN, float.NaN);
                        }
                    }
                case 1: //L1存在斜率, L2平行Y軸
                    {
                        float x = lineSecondStar.X;
                        float y = (lineFirstStar.X - x) * (-a) + lineFirstStar.Y;
                        return new PointF(x, y);
                    }
                case 2: //L1 平行Y軸，L2存在斜率
                    {
                        float x = lineFirstStar.X;
                        //網上有相似代碼的，這一處是錯誤的。你可以對比case 1 的邏輯 進行分析
                            //源code:lineSecondStar * x + lineSecondStar * lineSecondStar.X + p3.Y;
                        float y = (lineSecondStar.X - x) * (-b) + lineSecondStar.Y;
                        return new PointF(x, y);
                    }
                case 3: //L1，L2都存在斜率
                    {
                        if (a == b)
                        {
                            // throw new Exception("兩條直線平行或重合，無法計算交點。");
                            return new PointF(float.NaN, float.NaN);
                        }
                        float x = (a * lineFirstStar.X - b * lineSecondStar.X - lineFirstStar.Y + lineSecondStar.Y) / (a - b);
                        float y = a * x - a * lineFirstStar.X + lineFirstStar.Y;
                        return new PointF(x, y);
                    }
            }
            // throw new Exception("不可能發生的情況");
            return new PointF(float.NaN, float.NaN);
        }


        /// <summary>  
        /// 根据余弦定理求两个线段夹角  
        /// </summary>  
        /// <param name="o">端点</param>  
        /// <param name="s">start点</param>  
        /// <param name="e">end点</param>  
        /// <returns>Angle</returns>  
       public static double Angle(PointF o, PointF s, PointF e)  
        {  
            double cosfi = 0, fi = 0, norm = 0;  
            double dsx = s.X - o.X;  
            double dsy = s.Y - o.Y;  
            double dex = e.X - o.X;  
            double dey = e.Y - o.Y;  
        
            cosfi = dsx * dex + dsy * dey;  
            norm = (dsx * dsx + dsy * dsy) * (dex * dex + dey * dey);  
            cosfi /= Math.Sqrt(norm);  
        
            if (cosfi >= 1.0) return 0;  
            if (cosfi <= -1.0) return Math.PI;  
            fi = Math.Acos(cosfi);  
        
            if (180 * fi / Math.PI < 180)  
            {  
                return 180 * fi / Math.PI;  
            }  
            else  
            {  
                return 360 - 180 * fi / Math.PI;  
            }  
        }

        /// <summary>  
        /// 判斷兩直線所構成之夾角是否為銳角，其中判定方向為兩線夾護衛艦方向的夾角
        /// </summary>
        /// <param name="l1">線段1</param>  
        /// <param name="l2">線段2</param>  
        /// <param name="war_ship">護衛艦</param>  
        /// <returns>True:銳角;False:鈍角</returns>  
        public static bool IsAcuteAngle(Line l1, Line l2, Circle war_ship)
        {
            PointF intersectpoint = GetIntersection(l1.PointA, l1.PointB, l2.PointA, l2.PointB);
            PointF cutpoint_warship_l1;
            PointF cutpoint_warship_l2;
            
            if (Math.Abs(war_ship.radius - Distance(war_ship.center, l1.PointA)) <= 0.00001) cutpoint_warship_l1 = l1.PointA;
            else cutpoint_warship_l1 = l1.PointB;
            
            if (Math.Abs(war_ship.radius - Distance(war_ship.center, l2.PointA)) <= 0.00001) cutpoint_warship_l2 = l2.PointA;
            else cutpoint_warship_l2 = l2.PointB;
            
            double angle = Angle(intersectpoint, cutpoint_warship_l1, cutpoint_warship_l2);

            if (angle < 90) return true;
            else return false;
 

        }

        /// <summary>  
        /// 在tagent_lines中，兩切線夾角為銳角的切線中插入新的切線
        /// </summary>
        /// <param name="l1">線段1</param>
        /// <param name="l2">線段2</param>
        /// <param name="war_ship">護衛艦</param>
        /// <returns>Line l 切線</returns>
        public static Line NewCutLine(Line l1, Line l2, Circle war_ship, char return_side)
        {   
            // 兩切線交點
            PointF intersectpoint = GetIntersection(l1.PointA, l1.PointB, l2.PointA, l2.PointB);
            // 角平分線與威脅圓最近的交點
            PointF closestpointoncircle = ClosestIntersection(war_ship.center.X , war_ship.center.Y, war_ship.radius, 
                                                                intersectpoint, war_ship.center);
            
            // 護衛艦圓心到兩切線交點之單位向量
            System.Numerics.Vector2 bisector_vec = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(intersectpoint.X - war_ship.center.X, intersectpoint.Y - war_ship.center.Y));
            
            // 護衛艦圓心到兩切線交點之單位向量的 左or右 邊法向量
            System.Numerics.Vector2 bisector_normal_vec;
            if (return_side == 'R')
            {
                    bisector_normal_vec = new System.Numerics.Vector2(x:bisector_vec.Y, y:-bisector_vec.X);
            }
            else
            {
                bisector_normal_vec = new System.Numerics.Vector2(x:-bisector_vec.Y, y:bisector_vec.X);
            }

            // 法向量的另外一點
            PointF new_point_on_normal = new PointF(closestpointoncircle.X + 10 * bisector_normal_vec.X, 
                                                    closestpointoncircle.Y + 10 * bisector_normal_vec.Y);

            Line newcutline = new Line(closestpointoncircle, new_point_on_normal);

            return newcutline;

        }
        
        /// <summary>  
        /// 給定起始迴轉圓座標、目標轉折圓座標、dubin curve相關資訊，以及當前偵測到的所有護衛艦位置資訊，
        /// 尋求從起始迴轉圓至目標轉折圓中所有路徑，其迴轉圓圓心與迴轉方向
        /// </summary>
        /// <param name="startCenter">迴轉圓座標</param>
        /// <param name="goalCenter">目標圓座標</param>
        /// <param name="pathDataList">Dubin path曲線型態與切點等其他資訊</param>
        /// <param name="DetectedShips">偵測到的護衛艦</param>
        /// <returns>尋求從起始迴轉圓至目標轉折圓中所有路徑，其迴轉圓圓心與迴轉方向</returns>
        public static List<List<Tuple<Circle, char>>> AllReturnCircle(System.Numerics.Vector3 startCenter, System.Numerics.Vector3 goalCenter, OneDubinsPath pathDataList, List<System.Numerics.Vector3> DetectedShips)
        {
            float return_radius = 7.225f;
            float threaten_radius = 28.0f;

            //複製所有已觀測到的船艦
            List<System.Numerics.Vector3> ExecuteShips = new List<System.Numerics.Vector3>(DetectedShips);

            (Circle first_avoidance_circle, Line first_tangent_line) = FirstAvoidanceCircle(startCenter, goalCenter, pathDataList.tangent1, pathDataList.tangent2, 
                                                                ExecuteShips, return_radius, threaten_radius, pathDataList.pathType.ToString());

            if (first_avoidance_circle.center.X == goalCenter.X && first_avoidance_circle.center.Y == goalCenter.Z)
            {
                List<Tuple<Circle, char>> all_return_circles = new List<Tuple<Circle, char>>();
                all_return_circles.Add(new Tuple<Circle, char>(new Circle(new PointF(startCenter.X, startCenter.Z), return_radius), pathDataList.pathType.ToString()[0]));
                all_return_circles.Add(new Tuple<Circle, char>(new Circle(new PointF(goalCenter.X, goalCenter.Z), return_radius), pathDataList.pathType.ToString()[2]));
                List<List<Tuple<Circle, char>>> list_only_return_circles = new List<List<Tuple<Circle, char>>>();
                list_only_return_circles.Add(all_return_circles);
                return list_only_return_circles;
            }
            string first_avoid_to_goal_dubin_type = "";
            switch(pathDataList.pathType.ToString())
            {
                case "LSL":
                    first_avoid_to_goal_dubin_type = "RSL";
                    break;
                case "LSR":
                    first_avoid_to_goal_dubin_type = "RSR";
                    break;
                case "RSR":
                    first_avoid_to_goal_dubin_type = "LSR";
                    break;
                case "RSL":
                    first_avoid_to_goal_dubin_type = "LSL";
                    break;

                default: 
                    Console.WriteLine("WTF of the Dubin Type!!!"); 
                    break; 

            }

            // 尋求第一個威脅圓到最終目標圓的所有路徑，以AvoidanceTree樹狀結構儲存
            ExecuteShips = new List<System.Numerics.Vector3>(DetectedShips);
            List<AvoidanceTree> avoidance_circles = AvoidanceCircleToFinal(first_avoidance_circle, goalCenter, ExecuteShips, 
                                                                            return_radius, threaten_radius, first_avoid_to_goal_dubin_type);
            // 將第一個威脅圓與該切線，以及第一個威脅圓之後的樹狀路徑，整合成一個完整結構
            AvoidanceTree all_avoidance_ship = new AvoidanceTree(first_avoidance_circle, first_tangent_line, avoidance_circles);

            // 將樹狀結構拆解成各個完整路徑
            List<List<AvoidanceTree>> list_all_paths = new List<List<AvoidanceTree>>();
            // 透過公切線計算迴轉圓
            List<List<Tuple<Circle, char>>> list_all_return_circles = new List<List<Tuple<Circle, char>>>();

            // 透過ComputePaths輸出完整避障順序
            foreach(var path in ComputePaths(all_avoidance_ship, n=>n.next_stage))
            {
                // 儲存完整威脅圓順序
                List<AvoidanceTree> one_path = new List<AvoidanceTree>();
                // 威脅圓圓心座標
                List<Circle> avodiance_ship = new List<Circle>();
                // 切線，點A為當前威脅圓切點，點B為下一個威脅圓切點
                List<Line> tangent_line = new List<Line>();
                // 透過公切線計算迴轉圓及該圓旋轉方向
                List<Tuple<Circle, char>> all_return_circles = new List<Tuple<Circle, char>>();
                // 第一個迴轉圓為先前進行推算，不與任一威脅圓相割的圓
                all_return_circles.Add(new Tuple<Circle, char>(new Circle(new PointF(startCenter.X, startCenter.Z), return_radius), (char)pathDataList.pathType.ToString()[0]));

                foreach(var avoidance_circle in path)
                {
                    one_path.Add(avoidance_circle);
                    avodiance_ship.Add(avoidance_circle.avoid_circle);
                    tangent_line.Add(avoidance_circle.tangent_line);

                    // 若存入公切線的數量大於2，代表要開始計算迴轉圓
                    if (tangent_line.Count >= 2)
                    {
                        // 從倒數第二個圓與最後一個圓開始計算
                        int i = tangent_line.Count - 2;
                        while(i < tangent_line.Count - 1)
                        {
                            // 兩公切線的交點
                            PointF intersection = GetIntersection(tangent_line[i].PointA, tangent_line[i].PointB, 
                                                                    tangent_line[i+1].PointA, tangent_line[i+1].PointB);
                            // 第一條切線的向量
                            System.Numerics.Vector2 tangent_vec = new System.Numerics.Vector2(x:tangent_line[i].PointB.X - tangent_line[i].PointA.X, 
                                                            y:tangent_line[i].PointB.Y - tangent_line[i].PointA.Y);

                            char return_side;
                            return_side = (SideOfVector(tangent_line[i].PointA, tangent_line[i].PointB, tangent_line[i+1].PointA) == -1)?'R':'L';
                            
                            // 切點至交點的向量
                            System.Numerics.Vector2 cut_point_intersection = new System.Numerics.Vector2(x:intersection.X - tangent_line[i].PointA.X,
                                                                        y:intersection.Y - tangent_line[i].PointA.Y);
                            // 內積小於0(確切來說應是-1)，代表交點在航向的另一側，要在航向側產生新切線
                            if (System.Numerics.Vector2.Dot(tangent_vec, cut_point_intersection) < 0)
                            {
                                PointF intersection1;
                                PointF intersection2;
                                int intersections = FindLineCircleIntersections(avodiance_ship[i].center.X, avodiance_ship[i].center.Y, avodiance_ship[i].radius,
                                                                                 intersection, avodiance_ship[i].center, out intersection1, out intersection2);
                                
                                double dist1 = Distance(intersection1, intersection);
                                double dist2 = Distance(intersection2, intersection);
                                PointF new_cut_point = (dist1 > dist2)?intersection1:intersection2;

                                // 兩切線交點到護衛艦圓心之單位向量
                                System.Numerics.Vector2 bisector_vec = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(avodiance_ship[i].center.X - intersection.X,
                                                                                    avodiance_ship[i].center.Y - intersection.Y));
                                // 兩切線交點到護衛艦圓心之單位向量的左邊法向量
                                System.Numerics.Vector2 bisector_normal_vec = new System.Numerics.Vector2(-bisector_vec.Y, bisector_vec.X);

                                // 法向量的另外一點
                                PointF extend_point = new PointF(new_cut_point.X + 10 * bisector_normal_vec.X,
                                                                new_cut_point.Y + 10 * bisector_normal_vec.Y);
                                // 新公切線與第一條切線的交點
                                PointF new_tangent_intersection = GetIntersection(new_cut_point, extend_point, tangent_line[i].PointA, tangent_line[i].PointB);
                                
                                // 新公切線與第一條切線的交點 至 切點 的向量
                                System.Numerics.Vector2 new_tangent_intersection_vec = new System.Numerics.Vector2(new_cut_point.X - new_tangent_intersection.X,
                                                                                    new_cut_point.Y - new_tangent_intersection.Y);
                                
                                // 新切線的第二的點
                                PointF second_point_of_new_tanent = new PointF(new_cut_point.X + new_tangent_intersection_vec.X,
                                                                                new_cut_point.Y + new_tangent_intersection_vec.Y);

                                // 將新的公切線加入list中
                                tangent_line.Insert(i+1, new Line(new_cut_point, second_point_of_new_tanent));

                                // 將船艦複製
                                avodiance_ship.Insert(i+1, new Circle(avodiance_ship[i].center, avodiance_ship[i].radius));
                            }
                            // 若夾角為銳角，也需要重新產生公切線
                            else if(IsAcuteAngle(tangent_line[i], tangent_line[i+1], avodiance_ship[i]) == true)
                            {
                                tangent_line.Insert(i+1, NewCutLine(tangent_line[i], tangent_line[i+1], avodiance_ship[i], return_side));
                                avodiance_ship.Insert(i+1, new Circle(avodiance_ship[i].center, avodiance_ship[i].radius));
                            }
                            else
                            {
                                // 切線交點至護衛艦圓心連線
                                Line intersect_warship = new Line(intersection, avodiance_ship[i].center);
                                // 產生一個基準點
                                PointF base_point;
                                // 若前一個迴轉方向與當前的迴轉方向相反，代表使用內公切線
                                if (all_return_circles[all_return_circles.Count-1].Item2 != return_side)
                                {
                                    // 計算法向量
                                    System.Numerics.Vector2 normal_vec;
                                    if (return_side == 'R')
                                    {
                                         normal_vec = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(x:tangent_vec.Y, y:-tangent_vec.X));
                                    }
                                    else
                                    {
                                        normal_vec = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(x:-tangent_vec.Y, y:tangent_vec.X));
                                    }
                                    // 基點為前一個迴轉圓圓心加上兩倍回轉半徑的法向量
                                    base_point = new PointF(all_return_circles[all_return_circles.Count-1].Item1.center.X + 2 * return_radius * normal_vec.X,
                                                            all_return_circles[all_return_circles.Count-1].Item1.center.Y + 2 * return_radius * normal_vec.Y);
                                }
                                // 若前一個迴轉方向與當前相同，代表是外公切線，基點就是上一個迴轉圓
                                else
                                {
                                    base_point = all_return_circles[all_return_circles.Count-1].Item1.center;
                                }
                                // 延伸點為從基點延切線方向延伸的另一點
                                PointF extend_point = new PointF(base_point.X + 10 * tangent_vec.X,
                                                                base_point.Y + 10 * tangent_vec.Y);
                                // 透過基點與延伸點組成另一條線
                                Line preious_center_extension = new Line(base_point, extend_point);

                                // 切兩公切線的迴轉圓圓心
                                PointF return_center = GetIntersection(intersect_warship.PointA, intersect_warship.PointB, 
                                                                        preious_center_extension.PointA, preious_center_extension.PointB);

                                all_return_circles.Add(new Tuple<Circle, char>(new Circle(return_center, return_radius), return_side));
                                i += 1;
                                
                            }                            
                        }
                    }
                }
                all_return_circles.Add(new Tuple<Circle, char>(new Circle(new PointF(goalCenter.X, goalCenter.Z), return_radius), (char)pathDataList.pathType.ToString()[2]));
                list_all_paths.Add(one_path);
                list_all_return_circles.Add(all_return_circles);
            }

            return list_all_return_circles;
        
        }

        /// <summary>  
        /// 給定迴轉圓座標、目標圓座標、迴轉圓切點、目標圓切點、偵測到的護衛艦、回轉半徑、威脅半徑、dubin曲線型態
        /// 尋求以該dubin曲線型態從迴轉圓至目標圓，所要進行避障的第一個護衛艦位置，及迴轉圓至該威脅圓的切線
        /// </summary>
        /// <param name="start_center">迴轉圓座標</param>
        /// <param name="target_center">目標圓座標</param>
        /// <param name="tangent1">迴轉圓切點</param>
        /// <param name="tangent2">目標圓切點</param>
        /// <param name="DetectedShipsRemaining">偵測到的護衛艦</param>
        /// <param name="return_radius">回轉半徑</param>
        /// <param name="threaten_radius">威脅半徑</param>
        /// <param name="DubinType">dubin曲線型態</param>
        /// <returns>進行避障的第一個護衛艦位置，及迴轉圓至該威脅圓的切線</returns>
        public static (Circle, Line) FirstAvoidanceCircle(System.Numerics.Vector3 start_center, System.Numerics.Vector3 target_center, System.Numerics.Vector3 tangent1, System.Numerics.Vector3 tangent2, 
                                                    List<System.Numerics.Vector3>DetectedShipsRemaining, float return_radius, float threaten_radius, string DubinType)
        {
            // 此狀況出現在，迴轉圓沒有和任何一個護衛艦威脅圓相切於一點時
            // 此避障狀況為提前觸發，而不是看到護衛艦才觸發
            if (DetectedShipsRemaining.Count == 0)
            {
                PointF firstavoidancecircle = new PointF(x:target_center.X, y:target_center.Z);
                PointF cutpoint1 = new PointF(tangent1.X, tangent1.Z);
                PointF cutpoint2 = new PointF(tangent2.X, tangent2.Z);
                Line cutline = new Line(cutpoint1, cutpoint2);

                return (new Circle(firstavoidancecircle, threaten_radius), cutline);
            }

            float max_dist = 0.0f;
            int far_ship_indx = 0;

            // t1、t2為dubin曲線切點的座標
            PointF t1 = new PointF(tangent1.X, tangent1.Z);
            PointF t2 = new PointF(tangent2.X, tangent2.Z);
            for (int i = 0; i < DetectedShipsRemaining.Count; i++)
            {
                PointF ship_pos = new PointF(DetectedShipsRemaining[i].X, DetectedShipsRemaining[i].Z);

                PointF intersection1;
                PointF intersection2;

                // 計算當前切線與當前護衛艦威脅圓的交點，若交點數量為2，則兩交點為intersection1、intersection2
                int IntersectionNumbers = FindLineCircleIntersections(cx:ship_pos.X,
                                                                    cy:ship_pos.Y,
                                                                    radius:threaten_radius,
                                                                    point1: t1,
                                                                    point2: t2,
                                                                    out intersection1,
                                                                    out intersection2);
                // 兩切點距離
                double t1_t2 = Distance(t1, t2);
                // 交點1到切點1的距離
                double int1_t1 = Distance(intersection1, t1);
                // 交點1到切點2的距離
                double int1_t2 = Distance(intersection1, t2);
                // 交點2到切點1的距離
                double int2_t1 = Distance(intersection2, t1);
                // 交點2到切點2的距離
                double int2_t2 = Distance(intersection2, t2);

                // 判斷兩交點是否都在切線線段內，若皆在線段內，則該護衛艦則須進行避帳
                bool AvoidNewShip=false;
                if (Math.Abs(t1_t2 - (int1_t1 + int1_t2))<= 0.00001 && Math.Abs(t1_t2 - (int2_t1 + int2_t2)) <= 0.00001) AvoidNewShip=true;

                // 若交點數為2且接在切線線段內
                if (IntersectionNumbers == 2 && AvoidNewShip)
                {
                    // 船艦距離線段的投影距離
                    float project_dist = (float)pointToLine(l1: new Line(PointA:t1, PointB:t2), p1: ship_pos);
                    
                    // 判斷船艦在切線連線的左/右邊
                    int ship_side = SideOfVector(A:t1, B:t2, C:ship_pos);

                    char side_char;
                    if (ship_side == -1) side_char='R';
                    else side_char='L';

                    // 若船艦位置與避帳方向相反，則該投影距離要 減 一個威脅半徑
                    if (DubinType[0] != side_char) project_dist = threaten_radius - project_dist;
                    // 反之，則該投影距離要 加 一個威脅半徑
                    else project_dist = threaten_radius + project_dist;

                    // 若投影距離比當前紀錄的都大，則需要取該護衛艦作為首要避障圓
                    if (project_dist > max_dist) 
                    {
                        max_dist = project_dist;
                        far_ship_indx = i;
                    }
                }
            }

            // 若有求得新的護衛艦進行避障
            if (max_dist > 0.0f)
            {
                // 該護衛艦位置
                System.Numerics.Vector3 new_target_center = DetectedShipsRemaining[far_ship_indx];
                // 將該護衛艦座標作為新的目標圓圓心
                Circle new_goal_circle = new Circle(new PointF(x:new_target_center.X, y:new_target_center.Z), threaten_radius);
                // 起始迴轉圓圓心
                Circle start_circle = new Circle(new PointF(x:start_center.X, y:start_center.Z), return_radius);
                // 迴轉圓圓心與護衛艦圓心距離
                float start_goal_dist = (float)Distance(start_circle.center, new_goal_circle.center);
                // 若兩距離剛好等於28+7.225，代表兩圓相切，已是最優先進行避障的護衛艦，回傳該護衛艦
                if (Math.Abs(start_goal_dist - (return_radius + threaten_radius)) <= 0.00001)
                {
                    // 由於兩圓相切，兩圓內公切線為圓心向量的法向量
                    // 切點座標為回轉半徑與威脅半徑的比例
                    float w1 = threaten_radius/(threaten_radius + return_radius);
                    float w2 = return_radius/(threaten_radius + return_radius);
                    PointF cutpoint1 = new PointF(x:w1 * start_circle.center.X + w2 * new_goal_circle.center.X,
                                                y:w1 * start_circle.center.Y + w2 * new_goal_circle.center.Y);

                    // 起始迴轉圓至新目標圓的圓心向量
                    System.Numerics.Vector2 start_center_new_goal = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(x:new_goal_circle.center.X-start_circle.center.X,
                                                                                y:new_goal_circle.center.Y-start_circle.center.Y));
                    // 法向量
                    System.Numerics.Vector2 normal_vec;
                    if (DubinType[0] == 'L')
                    {
                        normal_vec = new System.Numerics.Vector2(x:-start_center_new_goal.Y, y:start_center_new_goal.X);
                    }
                    else
                    {
                        normal_vec = new System.Numerics.Vector2(x:start_center_new_goal.Y, y:-start_center_new_goal.X);
                    }
                    // 透過法向量與第一個切點推算第二個切點，作為內公切線的兩點
                    PointF cutpoint2 = new PointF(x:cutpoint1.X + return_radius * normal_vec.X, 
                                                y:cutpoint1.Y + return_radius * normal_vec.Y);
                    
                    Line cutline = new Line(cutpoint1, cutpoint2);
                    return (new_goal_circle, cutline);
                }
                // 若迴轉圓與護衛艦圓心距離大於28+7.225
                else
                {
                    // 則尋求內公切線，會有兩條內公切線
                    (Line l1, Line l2) = InnerTagentLines(start_circle, new_goal_circle);

                    // 尋求第1條內公切線上，威脅圓上的切點，其切點要與避障方向同邊
                    int l1_cutpoint_side = SideOfVector(A:t1, B:t2, C:l1.PointB);

                    char cutpoint_side_char;
                    if (l1_cutpoint_side == -1) cutpoint_side_char='R';
                    else cutpoint_side_char='L';

                    System.Numerics.Vector3 new_tangent1;
                    System.Numerics.Vector3 new_tangent2;
                    // 若切點方向與避障方向同邊，則代表該是第一條內公切線 l1
                    if (DubinType[0] == cutpoint_side_char)
                    {
                        new_tangent1 = new System.Numerics.Vector3(x:l1.PointA.X, y:0.0f, z:l1.PointA.Y);
                        new_tangent2 = new System.Numerics.Vector3(x:l1.PointB.X, y:0.0f, z:l1.PointB.Y);
                    }
                    // 若切點方向與避障方向反邊，則代表該是第二條內公切線 l2
                    else
                    {
                        new_tangent1 = new System.Numerics.Vector3(x:l2.PointA.X, y:0.0f, z:l2.PointA.Y);
                        new_tangent2 = new System.Numerics.Vector3(x:l2.PointB.X, y:0.0f, z:l2.PointB.Y);
                    }
                    
                    // 考慮過得護衛艦即刻刪除
                    DetectedShipsRemaining.RemoveAt(far_ship_indx);

                    // 
                    return FirstAvoidanceCircle(start_center, new_target_center, new_tangent1, new_tangent2, 
                                                DetectedShipsRemaining, return_radius, threaten_radius, DubinType);

                }

            }
            // 若新護衛艦圓心沒有被更新，則代表該圓就是首要最先避障的護衛艦
            else
            {
                PointF firstavoidancecircle = new PointF(x:target_center.X, y:target_center.Z);
                Line cutline = new Line(t1, t2);
                return (new Circle(firstavoidancecircle, threaten_radius), cutline);

            }
            
        }
        
        /// <summary>  
        /// 給定起始圓、目標圓與dubin曲線型態，則返回切線
        /// </summary>
        /// <param name="current_circle">起始圓</param>
        /// <param name="goal_circle">目標圓</param>
        /// <param name="DubinType">dubin曲線型態</param>
        /// <returns>切線</returns>
        public static Line ChooseTangentLine(Circle current_circle, Circle goal_circle, string DubinType)
        {
            // 計算護衛艦威脅圓與目標圓的切線
            Line tangent_line;
            if(DubinType[0] == DubinType[2])
            {
                // 計算外公切線
                (Line l1, Line l2) = OuterTagentLines(current_circle, goal_circle);

                // 計算l1目標圓切點在圓心向量的左邊或右邊
                int side = SideOfVector(current_circle.center, goal_circle.center, l1.PointB);

                // RSR 目標圓切點要在圓心向量的左邊
                if (DubinType[0] == 'R')
                {
                    if (side == 1) tangent_line = l1;
                    else tangent_line = l2; 
                }
                // LSL 目標圓切點要在圓心向量的右邊
                else
                {
                    if (side == -1) tangent_line = l1;
                    else tangent_line = l2;
                }
            }
            else
            {
                // 護衛艦威脅圓圓心與目標圓圓心距離
                float current_goal_dist = (float)Distance(current_circle.center, goal_circle.center);
                // 若兩距離剛好等於28+7.225，代表威脅圓圓心與目標圓圓心兩圓相切，已完成所有避障圓的排序
                if (Math.Abs(current_goal_dist - (current_circle.radius + goal_circle.radius)) <= 0.00001)
                {
                    // 由於兩圓相切，兩圓內公切線為圓心向量的法向量
                    // 切點座標為回轉半徑與威脅半徑的比例
                    float w1 = current_circle.radius/(current_circle.radius + goal_circle.radius);
                    float w2 = goal_circle.radius/(current_circle.radius + goal_circle.radius);
                    PointF cutpoint1 = new PointF(x:w1 * goal_circle.center.X + w2 * current_circle.center.X,
                                                y:w1 * goal_circle.center.Y + w2 * current_circle.center.Y);

                    // 護衛艦威脅圓至目標圓的圓心向量
                    System.Numerics.Vector2 current_center_goal_center = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(x:goal_circle.center.X-current_circle.center.X,
                                                                                y:goal_circle.center.Y-current_circle.center.Y));
                    // 法向量
                    System.Numerics.Vector2 normal_vec;
                    if (DubinType[0] == 'L')
                    {
                        normal_vec = new System.Numerics.Vector2(x:-current_center_goal_center.Y, y:current_center_goal_center.X);
                    }
                    else
                    {
                        normal_vec = new System.Numerics.Vector2(x:current_center_goal_center.Y, y:-current_center_goal_center.X);
                    }
                    // 透過法向量與第一個切點推算第二個切點，作為內公切線的兩點
                    PointF cutpoint2 = new PointF(x:cutpoint1.X + goal_circle.radius * normal_vec.X, 
                                                y:cutpoint1.Y + goal_circle.radius * normal_vec.Y);
                    
                    tangent_line = new Line(cutpoint1, cutpoint2);
                    
                }
                else
                {
                    // 計算內公切線
                    (Line l1, Line l2) = InnerTagentLines(current_circle, goal_circle);
                    
                    // 計算l1目標圓切點在圓心向量的左邊或右邊
                    int side = SideOfVector(current_circle.center, goal_circle.center, l1.PointB);

                    // RSL 目標圓切點要在圓心向量的右邊
                    if (DubinType[0] == 'R')
                    {
                        if (side == -1) tangent_line = l1;
                        else tangent_line = l2;

                    }
                    // LSR 目標圓切點要在圓心向量的左邊
                    else
                    {
                        if (side == 1) tangent_line = l1;
                        else tangent_line = l2;
                    }

                }
            }
            return tangent_line;
        }

        
        /// <summary>  
        /// 給定當前威脅圓與目標圓等相關參數，返回所有可能路徑的樹狀結構
        /// 此函式的目標圓座標固定為最終的迴轉圓，而當前威脅原則會一直修正
        /// </summary>
        /// <param name="current_circle">當前威脅圓</param>
        /// <param name="goalCenter">目標圓座標</param>
        /// <param name="DetectedShipsRemaining">偵測到的護衛艦</param>
        /// <param name="return_radius">回轉半徑</param>
        /// <param name="threaten_radius">威脅半徑</param>
        /// <param name="DubinType">dubin曲線型態</param>
        /// <returns>所有可能路徑的樹狀結構(威脅圓、切線、下一層路徑)</returns>
        public static List<AvoidanceTree> AvoidanceCircleToFinal(Circle current_circle, System.Numerics.Vector3 goalCenter, List<System.Numerics.Vector3>DetectedShipsRemaining,
                                                        float return_radius, float threaten_radius, string DubinType)                                    
        {
            // 把current_circle從DetectedShipsRemaining中刪除
            for (int i = DetectedShipsRemaining.Count-1; i >= 0; i--)
            {
                Circle current_ship = new Circle(new PointF(DetectedShipsRemaining[i].X, DetectedShipsRemaining[i].Z), threaten_radius);
                if(current_ship.center.Equals(current_circle.center))
                {
                    DetectedShipsRemaining.RemoveAt(i);
                    break;
                }
            }

            List<AvoidanceTree> AvoidanceCircles = new List<AvoidanceTree>();

            List<System.Numerics.Vector3> executed_detected_ship = new List<System.Numerics.Vector3>(DetectedShipsRemaining);

            Circle nearest_ship = GetNearestShipToAvoid(current_circle, goalCenter, executed_detected_ship, return_radius, threaten_radius, DubinType);
            
            for (int i = executed_detected_ship.Count-1; i >= 0; i--)
            {
                Circle current_ship = new Circle(new PointF(executed_detected_ship[i].X, executed_detected_ship[i].Z), threaten_radius);
                if(current_ship.center.Equals(nearest_ship.center))
                {
                    executed_detected_ship.RemoveAt(i);
                    break;
                }
            }

            // 如果最接近的目標圓已經是最終的目標圓，則代表已完成所有避障圓的排序
            if (nearest_ship.center.Equals(new PointF(goalCenter.X, goalCenter.Z)))
            {
                Circle goal_circle = new Circle(new PointF(goalCenter.X, goalCenter.Z), return_radius);
                Line tangent_line_final = ChooseTangentLine(current_circle, goal_circle, DubinType);

                AvoidanceCircles.Add(new AvoidanceTree(nearest_ship, tangent_line_final, null));
                return AvoidanceCircles;
            }
            // 如果當前威脅圓至目標圓之間有新護衛艦須進行避障
            else
            {
                // 必定存在外公切線
                string current_dubin_type;
                // 如果當前的護衛艦至目標圓的dubin曲線是RS(R/L)，則當前護衛艦至新的護衛艦的dubin曲線是RSR
                if (DubinType[0]=='R') current_dubin_type = "RSR";
                // 如果當前的護衛艦至目標圓的dubin曲線是LS(R/L)，則當前護衛艦至新的護衛艦的dubin曲線是LSL
                else current_dubin_type = "LSL";

                Circle nearest_ship_outer = new Circle(nearest_ship.center, nearest_ship.radius);
                // 計算當前護衛艦至新的護衛艦(新的目標圓)的外公切線
                Line tangent_line_outer = ChooseTangentLine(current_circle, nearest_ship_outer, current_dubin_type);
                List<System.Numerics.Vector3> executed_detected_ship_outer = new List<System.Numerics.Vector3>(executed_detected_ship);
                while(executed_detected_ship_outer.Count > 0)
                {
                    Circle nearest_ship_outer_org = new Circle(nearest_ship_outer.center, nearest_ship_outer.radius);

                    System.Numerics.Vector3 nearest_ship_center = new System.Numerics.Vector3(x:nearest_ship_outer.center.X, y:0.0f, z:nearest_ship_outer.center.Y);
                    nearest_ship_outer = GetNearestShipToAvoid(current_circle, nearest_ship_center, executed_detected_ship_outer, 
                                                                                    nearest_ship_outer.radius, threaten_radius, current_dubin_type);
                    tangent_line_outer = ChooseTangentLine(current_circle, nearest_ship_outer, current_dubin_type);
                    if (nearest_ship_outer.center.Equals(nearest_ship_outer_org.center))
                    {
                        break;
                    }
                    else
                    {
                        for (int i = executed_detected_ship_outer.Count-1; i >= 0; i--)
                        {
                            Circle current_ship = new Circle(new PointF(executed_detected_ship_outer[i].X, executed_detected_ship_outer[i].Z), threaten_radius);
                            if(current_ship.center.Equals(nearest_ship_outer.center))
                            {
                                executed_detected_ship_outer.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }

                // 計算下一階層的避障路徑，dubin曲線會和原本的相同
                List<AvoidanceTree> next_stage_outer = AvoidanceCircleToFinal(nearest_ship_outer, goalCenter, DetectedShipsRemaining, return_radius, threaten_radius, DubinType);

                // 將新的避障圓、外公切線、下一階層的避帳路徑加入List中
                AvoidanceCircles.Add(new AvoidanceTree(nearest_ship_outer, tangent_line_outer, next_stage_outer));


                // 檢查是否存在內公切線，兩圓距離大於2倍威脅半徑，則存在內公切線
                float comparison = threaten_radius * 2f;
                float center_dist = (float)Distance(current_circle.center, nearest_ship.center);
                if (center_dist > comparison)
                {
                    string next_dubin_type;
                    // 若原本的dubin曲線是RS(R/L)
                    if (DubinType[0]=='R') 
                    {
                        // 當前護衛艦至新的護衛艦(新的目標圓)的dubin曲線是RSL
                        current_dubin_type = "RSL";
                        // 新的護衛艦至舊的目標圓的dubin曲線是LS(R/L)
                        next_dubin_type = "LS" + DubinType[2];
                    }
                    // 若原本的dubin曲線是LS(R/L)
                    else
                    {
                        // 當前護衛艦至新的護衛艦(新的目標圓)的dubin曲線是LSR
                        current_dubin_type = "LSR";
                        // 新的護衛艦至舊的目標圓的dubin曲線是RS(R/L)
                        next_dubin_type = "RS" + DubinType[2];
                    }

                Circle nearest_ship_inner = new Circle(nearest_ship.center, nearest_ship.radius);
                // 計算當前護衛艦至新的護衛艦(新的目標圓)的內公切線
                Line tangent_line_inner = ChooseTangentLine(current_circle, nearest_ship_inner, current_dubin_type);
                List<System.Numerics.Vector3> executed_detected_ship_inner = new List<System.Numerics.Vector3>(executed_detected_ship);
                while(executed_detected_ship_inner.Count > 0)
                {
                    Circle nearest_ship_inner_org = new Circle(nearest_ship_inner.center, nearest_ship_inner.radius);

                    System.Numerics.Vector3 nearest_ship_center = new System.Numerics.Vector3(x:nearest_ship_inner.center.X, y:0.0f, z:nearest_ship_inner.center.Y);
                    nearest_ship_inner = GetNearestShipToAvoid(current_circle, nearest_ship_center, executed_detected_ship_inner, 
                                                                                    nearest_ship_inner.radius, threaten_radius, current_dubin_type);
                    tangent_line_inner = ChooseTangentLine(current_circle, nearest_ship_inner, current_dubin_type);
                    if (nearest_ship_inner.center.Equals(nearest_ship_inner_org.center))
                    {
                        break;
                    }
                    else
                    {
                        for (int i = executed_detected_ship_inner.Count-1; i >= 0; i--)
                        {
                            Circle current_ship = new Circle(new PointF(executed_detected_ship_inner[i].X, executed_detected_ship_inner[i].Z), threaten_radius);
                            if(current_ship.center.Equals(nearest_ship_inner.center))
                            {
                                executed_detected_ship_inner.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }

                    // 計算下一階層的避障路徑，dubin曲線為next_dubin_type
                    List<AvoidanceTree> next_stage_inner = AvoidanceCircleToFinal(nearest_ship_inner, goalCenter, DetectedShipsRemaining, return_radius, threaten_radius, next_dubin_type);
                    // 將新的避障圓、外公切線、下一階層的避帳路徑加入List中
                    AvoidanceCircles.Add(new AvoidanceTree(nearest_ship_inner, tangent_line_inner, next_stage_inner));

                }

                return AvoidanceCircles;

            }
            
        }

        public static Circle GetNearestShipToAvoid(Circle current_circle, System.Numerics.Vector3 goalCenter, List<System.Numerics.Vector3>executed_detected_ship,
                                                        float goal_radius, float threaten_radius, string DubinType)
        {
            // 目標圓
            Circle goal_circle = new Circle(new PointF(goalCenter.X, goalCenter.Z), goal_radius);

            // 計算護衛艦威脅圓與目標圓的切線
            Line tangent_line = ChooseTangentLine(current_circle, goal_circle, DubinType);
            
            // 兩切點的距離
            float tangent_points_dist = (float)Distance(tangent_line.PointA, tangent_line.PointB);

            // 切點航向的單位向量
            System.Numerics.Vector2 tangent_line_vec = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(tangent_line.PointB.X-tangent_line.PointA.X, 
                                                                    tangent_line.PointB.Y-tangent_line.PointA.Y));
            // 當前威脅圓至目標迴轉圓最近的船艦預設為目標迴轉圓
            float min_dist = float.MaxValue;
            Circle nearest_ship = new Circle(goal_circle.center, goal_circle.radius);

            // 所有剩下的護衛艦要一一檢測是否存在於切線線段內
            for (int i=executed_detected_ship.Count-1; i>=0; i--)
            {
                // 護衛艦威脅圓與切線的交點數
                PointF intersection1;
                PointF intersection2;
                int intersection = FindLineCircleIntersections(executed_detected_ship[i].X, executed_detected_ship[i].Z, threaten_radius,
                                                                tangent_line.PointA, tangent_line.PointB, out intersection1, out intersection2);
                                                                
                
                // PointF ProjectivePoint = LinePointProjection(tangent_line.PointA, tangent_line.PointB, 
                //                                             new PointF(executed_detected_ship[i].X, executed_detected_ship[i].Z));

                // System.Numerics.Vector2 CutPoint1_ProPoint = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(ProjectivePoint.X-tangent_line.PointA.X, 
                //                                                             ProjectivePoint.Y-tangent_line.PointA.Y));

                // float ProPoint_tangent1 = (float)Distance(ProjectivePoint, tangent_line.PointA);
                // float ProPoint_tangent2 = (float)Distance(ProjectivePoint, tangent_line.PointB);

                // 交點1與切點1的距離
                float inter1_t1 = (float)Distance(intersection1, tangent_line.PointA);
                // 交點1與切點2的距離
                float inter1_t2 = (float)Distance(intersection1, tangent_line.PointB);
                // 交點1與切點1的距離
                float inter2_t1 = (float)Distance(intersection2, tangent_line.PointA);
                // 交點2與切點2的距離
                float inter2_t2 = (float)Distance(intersection2, tangent_line.PointB);

                bool AvoidNewShip=false;
                float ProPoint_tangent1 = float.MaxValue;

                // 若兩交點中有其一交點位於切點連線之間，則該護衛艦需要進行避障
                if (Math.Abs(tangent_points_dist - (inter1_t1 + inter1_t2))<= 0.00001 ||
                    Math.Abs(tangent_points_dist - (inter2_t1 + inter2_t2))<= 0.00001)
                    {

                        AvoidNewShip = true;
                        // 船艦位置於切線線段的投影點
                        PointF ProjectivePoint = LinePointProjection(tangent_line.PointA, tangent_line.PointB, 
                                                                new PointF(executed_detected_ship[i].X, executed_detected_ship[i].Z));
                        
                        // 威脅圓切點至投影點的向量
                        System.Numerics.Vector2 CutPoint1_ProPoint = System.Numerics.Vector2.Normalize(new System.Numerics.Vector2(ProjectivePoint.X-tangent_line.PointA.X, 
                                                                                    ProjectivePoint.Y-tangent_line.PointA.Y));

                        // 投影點至威脅圓切點的距離
                        ProPoint_tangent1 = (float)Distance(ProjectivePoint, tangent_line.PointA);
                        
                        // 投影點距離威脅圓切點的距離，若投影點在切點左側，則距離變負的，要取最小的護衛艦進行避障
                        if (System.Numerics.Vector2.Dot(CutPoint1_ProPoint, tangent_line_vec) < 0) ProPoint_tangent1 = -ProPoint_tangent1;
                        
                    }

                if ( intersection == 2 && AvoidNewShip)
                {
                    // 要取距離切點1最小的護衛艦進行避障
                    if (ProPoint_tangent1 < min_dist)
                    {
                        min_dist = ProPoint_tangent1;
                        nearest_ship = new Circle(new PointF(executed_detected_ship[i].X, executed_detected_ship[i].Z), threaten_radius);
                    }
                }

            }
            return nearest_ship;
            
        }

        public static IEnumerable<IEnumerable<T>> ComputePaths<T>(T Root, Func<T, IEnumerable<T>> Children)
        {
            var next_stages = Children(Root);
            if (next_stages != null && next_stages.Any())
            {
                foreach (var one_path in next_stages)
                {
                    foreach (var next_next_stage in ComputePaths(one_path, Children))
                    {
                        yield return new[] {Root}.Concat(next_next_stage);
                    }
                }
            }
            else
            {
                yield return new[] {Root};
            }
        }

    }

}