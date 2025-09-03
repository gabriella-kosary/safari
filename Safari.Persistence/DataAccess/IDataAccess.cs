using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safari.Persistence.DataAccess {
    public interface IDataAccess {
        public GameTable Load(String path);
        public void Save(GameTable gameTable, String path);
    }
}
