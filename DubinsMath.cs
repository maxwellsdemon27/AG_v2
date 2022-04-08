using System;
using System.Drawing;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;

namespace DubinsPathsTutorial
{
    //To keep track of the different paths when debugging
    public enum PathType { RSR, LSL, RSL, LSR, RLR, LRL }


    //Takes care of all standardized methods related the generating of Dubins paths
    public static class DubinsMath
    {
        //How far we are driving each update, the accuracy will improve if we lower the driveDistance
        //But not too low because rounding errors will appear
        //Is used to generate the coordinates of a path
        public static float driveDistance = 0.02f;
        //The radius the car can turn 360 degrees with
        public static float turningRadius = 7.225f;


        //Calculate center positions of the Right circle
        public static Vector3 GetRightCircleCenterPos(Vector3 carPos, float heading)
        {
            Vector3 rightCirclePos = Vector3.Zero;

            //The circle is 90 degrees (pi/2 radians) to the right of the car's heading
            rightCirclePos.X = carPos.X + turningRadius * MathF.Sin(heading + (MathF.PI / 2f));
            rightCirclePos.Z = carPos.Z + turningRadius * MathF.Cos(heading + (MathF.PI / 2f));

            return rightCirclePos;
        }


        //Calculate center positions of the Left circle
        public static Vector3 GetLeftCircleCenterPos(Vector3 carPos, float heading)
        {
            Vector3 rightCirclePos = Vector3.Zero;

            //The circle is 90 degrees (pi/2 radians) to the left of the car's heading
            rightCirclePos.X = carPos.X + turningRadius * MathF.Sin(heading - (MathF.PI / 2f));
            rightCirclePos.Z = carPos.Z + turningRadius * MathF.Cos(heading - (MathF.PI / 2f));

            return rightCirclePos;
        }


        //
        // Calculate the start and end positions of the tangent lines
        //

        //Outer tangent (LSL and RSR)
        public static void LSLorRSR(
            Vector3 startCircle,
            Vector3 goalCircle,
            bool isBottom,
            out Vector3 startTangent,
            out Vector3 goalTangent)
        {
            //The angle to the first tangent coordinate is always 90 degrees if the both circles have the same radius
            float theta = 90f * (MathF.PI * 2) / 360;

            //Need to modify theta if the circles are not on the same height (z)
            theta += MathF.Atan2(goalCircle.Z - startCircle.Z, goalCircle.X - startCircle.X);

            //Add pi to get the "bottom" coordinate which is on the opposite side (180 degrees = pi)
            if (isBottom)
            {
                theta += MathF.PI;
            }

            //The coordinates of the first tangent points
            float xT1 = startCircle.X + turningRadius * MathF.Cos(theta);
            float zT1 = startCircle.Z + turningRadius * MathF.Sin(theta);

            //To get the second coordinate we need a direction
            //This direction is the same as the direction between the center pos of the circles
            Vector3 dirVec = goalCircle - startCircle;

            float xT2 = xT1 + dirVec.X;
            float zT2 = zT1 + dirVec.Z;

            //The final coordinates of the tangent lines
            startTangent = new Vector3(xT1, 0f, zT1);

            goalTangent = new Vector3(xT2, 0f, zT2);
        }


        //Inner tangent (RSL and LSR)
        public static void RSLorLSR(
            Vector3 startCircle,
            Vector3 goalCircle,
            bool isBottom,
            out Vector3 startTangent,
            out Vector3 goalTangent)
        {
            //Find the distance between the circles
            float D = (startCircle - goalCircle).Length();

            //If the circles have the same radius we can use cosine and not the law of cosines 
            //to calculate the angle to the first tangent coordinate 
            float theta = MathF.Acos((2f * turningRadius) / D);

            //If the circles is LSR, then the first tangent pos is on the other side of the center line
            if (isBottom)
            {
                theta *= -1f;
            }

            //Need to modify theta if the circles are not on the same height            
            theta += MathF.Atan2(goalCircle.Z - startCircle.Z, goalCircle.X - startCircle.X);

            //The coordinates of the first tangent point
            float xT1 = startCircle.X + turningRadius * MathF.Cos(theta);
            float zT1 = startCircle.Z + turningRadius * MathF.Sin(theta);

            //To get the second tangent coordinate we need the direction of the tangent
            //To get the direction we move up 2 circle radius and end up at this coordinate
            float xT1_tmp = startCircle.X + 2f * turningRadius * MathF.Cos(theta);
            float zT1_tmp = startCircle.Z + 2f * turningRadius * MathF.Sin(theta);

            //The direction is between the new coordinate and the center of the target circle
            Vector3 dirVec = goalCircle - new Vector3(xT1_tmp, 0f, zT1_tmp);

            //The coordinates of the second tangent point is the 
            float xT2 = xT1 + dirVec.X;
            float zT2 = zT1 + dirVec.Z;

            //The final coordinates of the tangent lines
            startTangent = new Vector3(xT1, 0f, zT1);

            goalTangent = new Vector3(xT2, 0f, zT2);
        }


        //Get the RLR or LRL tangent points
        public static void GetRLRorLRLTangents(
            Vector3 startCircle,
            Vector3 goalCircle,
            bool isLRL,
            out Vector3 startTangent,
            out Vector3 goalTangent,
            out Vector3 middleCircle)
        {
            //The distance between the circles
            float D = (startCircle - goalCircle).Length();;

            //The angle between the goal and the new 3rd circle we create with the law of cosines
            float theta = MathF.Acos(D / (4f * turningRadius));

            //But we need to modify the angle theta if the circles are not on the same line
            Vector3 V1 = goalCircle - startCircle;

            //Different depending on if we calculate LRL or RLR
            if (isLRL)
            {
                theta = MathF.Atan2(V1.Z, V1.X) + theta;
            }
            else
            {
                theta = MathF.Atan2(V1.Z, V1.X) - theta;
            }

            //Calculate the position of the third circle
            float x = startCircle.X + 2f * turningRadius * MathF.Cos(theta);
            float y = startCircle.Y;
            float z = startCircle.Z + 2f * turningRadius * MathF.Sin(theta);

            middleCircle = new Vector3(x, y, z);

            //Calculate the tangent points
            Vector3 V2 = Vector3.Normalize(startCircle - middleCircle);
            Vector3 V3 = Vector3.Normalize(goalCircle - middleCircle);

            startTangent = middleCircle + V2 * turningRadius;
            goalTangent = middleCircle + V3 * turningRadius;
        }


        //Calculate the length of an circle arc depending on which direction we are driving
        public static float GetArcLength(
            Vector3 circleCenterPos,
            Vector3 startPos,
            Vector3 goalPos,
            bool isLeftCircle)
        {
            Vector3 V1 = startPos - circleCenterPos;
            Vector3 V2 = goalPos - circleCenterPos;

            float theta = MathF.Atan2(V2.Z, V2.X) - MathF.Atan2(V1.Z, V1.X);

            if (theta < 0f && isLeftCircle)
            {
                theta += 2f * MathF.PI;
            }
            else if (theta > 0 && !isLeftCircle)
            {
                theta -= 2f * MathF.PI;
            }

            float arcLength = MathF.Abs(theta * turningRadius);

            return arcLength;
        }


        //Loops through segments of a path and add new coordinates to the final path
        public static void AddCoordinatesToPath(
            ref Vector3 currentPos,
            ref float theta,
            List<Vector3> finalPath,
            int segments,
            bool isTurning,
            bool isTurningRight)
        {
            for (int i = 0; i < segments; i++)
            {
                //Update the position of the car
                currentPos.X += driveDistance * MathF.Sin(theta);
                currentPos.Z += driveDistance * MathF.Cos(theta);

                //Don't update the heading if we are driving straight
                if (isTurning)
                {
                    //Which way are we turning?
                    float turnParameter = 1f;

                    if (!isTurningRight)
                    {
                        turnParameter = -1f;
                    }

                    //Update the heading
                    theta += (driveDistance / turningRadius) * turnParameter;
                }

                //Add the new coordinate to the path
                finalPath.Add(currentPos);
            }
        }
    }
}