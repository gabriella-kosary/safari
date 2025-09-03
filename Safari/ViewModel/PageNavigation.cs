using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Safari.ViewModel {
    public class PageNavigation {
        public static event Action<Page>? NavigationRequested;

        public static void RequestNavigation(Page pageType) {
            NavigationRequested?.Invoke(pageType);
        }
    }
}
