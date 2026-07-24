using System;

namespace COServer.Game.MsgServer.AttackHandler.Algoritms
{
    /*public class Sector
    {
        const int rangePerAngle = 15;
        private static HashSet<int> createSector(int attackAngle, int angle)
        {
            HashSet<int> allowedAngles = new HashSet<int>() { attackAngle };
            int angleX = angle / 2, angleZ = attackAngle;
            while (angleX > 0)
            {
                angleZ += 1;
                angleX -= 1;
                if (angleZ == 360) angleZ = 0;
                allowedAngles.Add(angleZ);
            }
            angleX = angle / 2;
            angleZ = attackAngle;
            while (angleX > 0)
            {
                angleZ -= 1;
                angleX -= 1;
                if (angleZ == -1) angleZ = 359;
                allowedAngles.Add(angleZ);
            }
            return allowedAngles;
        }

        private int attackerX, attackerY;
        private int distance;
        private int attackAngle;
        private HashSet<int> allowedAngles;

        public Sector(int attackerX, int attackerY, int attackX, int attackY, int range, int distance)
        {
            this.attackerX = attackerX;
            this.attackerY = attackerY;
            this.distance = distance;
            attackAngle = Kernel.GetDegree(attackerX, attackerY, attackX, attackY);
            int angle = range * rangePerAngle;
            allowedAngles = createSector(attackAngle, angle);
        }

        public bool Inside(int X, int Y)
        {
            if (Kernel.GetDistance((ushort)X, (ushort)Y, (ushort)attackerX, (ushort)attackerY) <= distance)
            {
                var degree = Kernel.GetDegree(attackerX, attackerY, X, Y);
                return allowedAngles.Contains(degree);
            }
            return false;
        }

        public int Difference(int X, int Y)
        {
            var degree = Kernel.GetDegree(attackerX, attackerY, X, Y);
            var leftDifference = ldif(attackAngle, degree);
            var rightDifference = rdif(attackAngle, degree);
            return Math.Min(leftDifference, rightDifference);
        }

        private int rdif(int attackAngle, int degree)
        {
            int counter = 0;
            while (degree != attackAngle)
            {
                counter++;
                degree++;
                if (degree == 360) degree = 0;
            }
            return counter;
        }

        private int ldif(int attackAngle, int degree)
        {
            int counter = 0;
            while (degree != attackAngle)
            {
                counter++;
                degree--;
                if (degree == 0) degree = 360;
            }
            return counter;
        }
     *         public static int GetDegree(int X, int Y, int X2, int Y2)
        {
            double angle = Math.Atan2(Y2 - Y, X2 - X);
            if (angle < 0) angle += Math.PI * 2;
            return (int)Math.Round(angle * 180 / Math.PI);
        }
    }*/
    public class Sector
    {
        private int attackerX, attackerY, attackX, attackY;
        private int degree, sectorsize, leftside, rightside;
        private int distance;
        private bool addextra;

        public Sector(int attackerX, int attackerY, int attackX, int attackY)
        {
            this.attackerX = attackerX;
            this.attackerY = attackerY;
            this.attackX = attackX;
            this.attackY = attackY;
            this.degree = (int)Role.Core.GetAngle((ushort)attackerX, (ushort)attackX, (ushort)attackerY, (ushort)attackY);
            this.addextra = false;
        }

        public void Arrange(int sectorsize, int distance)
        {
            this.distance = Math.Min(distance, 18);//14
            this.sectorsize = sectorsize;
            this.leftside = this.degree - (sectorsize / 2);
            if (this.leftside < 0)
                this.leftside += 360;
            this.rightside = this.degree + (sectorsize / 2);
            if (this.leftside < this.rightside || this.rightside - this.leftside != this.sectorsize)
            {
                this.rightside += 360;
                this.addextra = true;
            }
        }


        public bool Inside(int X, int Y)
        {
            if (Role.Core.GetDistance((ushort)X, (ushort)Y, (ushort)attackerX, (ushort)attackerY) <= distance)
            {
                int degree = (int)Role.Core.GetAngle((ushort)attackerX, (ushort)X, (ushort)attackerY, (ushort)Y);
                if (this.addextra)
                    degree += 360;
                if (degree >= this.leftside && degree <= this.rightside)
                    return true;
            }
            return false;
        }
    }
}
