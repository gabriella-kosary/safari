using Safari.Persistence.Entities;
using Safari.Persistence.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safari.Persistence {
	public class TilesCreatedEventArgs : EventArgs {
		private Tile[,]? _tiles;
		private int _canvasWidth;
		private int _canvasHeight;
		private List<Entity>? _entities;
		private (int, int) _startPosition;
		private (int, int) _endPosition;

		public (int, int) StartPosition {
			get { return _startPosition; }
			set { _startPosition = value; }
		}
		public (int, int) EndPosition {
			get { return _endPosition; }
			set { _endPosition = value; }
		}
		public int CanvasHeight {
			get { return _canvasHeight; }
			set { _canvasHeight = value; }
		}
		public int CanvasWidth {
			get { return _canvasWidth; }
			set { _canvasWidth = value; }
		}
		public Tile[,] Tiles {
			get { return _tiles ?? new Tile[0,0]; }
			set { _tiles = value; }
		}
		public List<Entity> Enitites {
			get { return _entities ?? new List<Entity>(); }
			set { _entities = value; }
		}
	}
}
