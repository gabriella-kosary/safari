using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safari.Persistence {
    public class StatEventArgs : EventArgs {
		private int _carnivores;
		private int _herbivores;
		private int _visitors;

		public int Visitors {
			get { return _visitors; }
			set { _visitors = value; }
		}
		public int Herbivores {
			get { return _herbivores; }
			set { _herbivores = value; }
		}
		public int Carnivores {
			get { return _carnivores; }
			set { _carnivores = value; }
		}
	}
}
