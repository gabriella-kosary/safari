using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Safari.Persistence.Entities
{
    public class Jeep : Entity
    {
        public List<(int, int)> path { get; set; }
        public int passengers { get; set; }
        public int pathIndex { get; set; }
        public bool onThePath { get; set; }

        public Jeep((int, int) position, int fov)
        {
            canSee = true;
            this.position = position;
            this.fov = fov;
            passengers = 0;
            pathIndex = -1; //0-tól path.count-1 ig, éppen hol jár az úton
            path = new List<(int, int)>();
            onThePath = false;
        }
        public void GiveJeepPath(List<(int, int)> path)
        {
            if(path.Count != 0) {
                this.path = path;
                pathIndex = 0;
                destination = path[pathIndex];
            }
        }
        #region ProtectedFunctions
        protected override void NewDestination(int canvasWidth, int canvasHeight, int tileWidth, int tileHeight)
        {
            if (path.Count <= pathIndex && passengers == 4) //végig ment az útján, és leadja az utasokat
            {
                passengers = 0;
                path.Reverse();
                pathIndex = 0;
            }
            else if (path.Count <= pathIndex && passengers == 0) //visszatért a kezdőpontba
            {
                path.Clear();
                pathIndex = -1;
                onThePath = false;
            } else if(pathIndex == - 1) {
                onThePath = false;
            }
            else if (onThePath) //ha éppen mozgásban van
            {
                destination = path[pathIndex];
                (int, int) poz = position;
                pathIndex++;
            }
        }
        #endregion

        #region PrivateFunctions
        #endregion
    }
}
