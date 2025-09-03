using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Safari.ViewModel
{
    public class TutorialViewModel : ViewModelBase {
        public string TutorialText => "A játék egy afrikai szafari park igazgatását mutatja be, felülnézetes 2D pályán.\r\n\r\nAlapvető Játékmechanizmusok:\r\n    • Cél: A szafari park sikeres működtetése.\r\n        A tőke, az állatok és a turisták számának növelése, valamint bizonyos ideig a nehézségi szinttől függő küszöbértékek fölött tartása.\r\n        A játék elvesztését a csőd vagy az összes állat kipusztulása jelenti.\r\n    • Időirányítás: A felhasználói felületen található sebességgombokkal az idő múlásának sebessége módosítható (óra / nap / hét).\r\n        A napok száma a felhasználói felületen látható.\r\n    • Tőke: Kezdőtőkével indul a játék. A pénz felhasználható vásárlásokra, például:\r\n        növények, állatok, dzsippek, utak, vadőrök, chipek.\r\n        Fenntartási költségekre is szükség van (pl. vadőrök fizetése).\r\n        Bevételhez vezet az állatok eladása és a turisták általi dzsippbérlés.\r\n        Az aktuális tőke a felhasználói felület jobb felső részén jelenik meg.\r\n    • Vásárlás: A vásárolni kívánt elemeket a legördülő menüben lehet kiválasztani.\r\n        A vásárláshoz a bevásárlókocsi (\U0001f6d2) ikonnal jelölt gomb használható.\r\n        A gomb megnyomásával a vásárlási mód aktiválódik (a gomb szürkén jelenik meg).\r\n        Ezt követően a térképen egy mezőre kattintva az aktuálisan kiválasztott elem lehelyezhető.\r\n        A vásárlási mód kikapcsolása a gomb újabb megnyomásával történik.\r\n\r\nKiegészítő Funkciók:\r\n    • Terepi Akadályok:\r\n        ◦ A térképen terepi akadályok is találhatók: dombok és folyók.\r\n        ◦ Ezek az akadályok az állatok mozgását lassítják, és az útvonalválasztást befolyásolják.\r\n        ◦ A dombokról nagyobb távolságra ellátnak az állatok.\r\n        ◦ A folyók ivóvízforrásként szolgálnak.\r\n    • Napszakok:\r\n        ◦ A játékban váltakoznak a napszakok.\r\n        ◦ Éjjel a térkép csak azon részei láthatók, ahova növényeket, vizet vagy utat helyeztek el.\r\n        ◦ Az állatok éjjel csak akkor láthatók, ha turisták vagy vadőrök a közelükben vannak, vagy ha helymeghatározó chip van rajtuk.\r\n        ◦ Az orvvadászok és a vadőrök éjjel is aktívak.\r\n    • Vadőrök és Irányítható Vadőrök:\r\n        ◦ Vadőrök vásárolhatók (\"Ranger\" a listában).\r\n        ◦ A vadőrök havonta kapnak fizetést.\r\n        ◦ A vadőrök állatok kilövésére használhatók. A lelőtt állatért pénz jár.\r\n        ◦ Az orvvadászok elleni védekezésben is részt vesznek: ha egy orvvadász a közelükbe kerül, a vadőr megpróbálja lelőni.\r\n        ◦ Irányított kilövés: A \"kill\" mód a \U0001f9b9 gombbal aktiválható. Ezután egy vadőr kiválasztható a térképen.\r\n          Végül egy célpontra (állat vagy észlelt orvvadász) kattintva a vadőr elkezdi üldözni, majd megkísérli lelőni azt.\r\n    • Orvvadászok:\r\n        ◦ Az orvvadászok megjelenhetnek a parkban, és megpróbálhatják az állatokat kilőni vagy elfogni.\r\n        ◦ Az orvvadászok csak akkor láthatók a térképen, ha turisták vagy vadőrök vannak a közelükben.\r\n        ◦ Védekezéshez vadőrök alkalmazhatók.\r\n        ◦ Ha meghal az orvvadász, akkor az elfogott állatok kiszabadulnak.\r\n\r\nEgyéb UI Elemek:\r\n    • Az \"Új játék\" gombbal új játék indítható.\r\n    • A \"Kilépés\" gombbal a játékból való kilépés lehetséges.";
        public DelegateCommand? BackCommand { get; set; }
        public String? Level { get; set; }

        public event EventHandler<(String, String, bool)>? BackEvent;

        public TutorialViewModel() {
            BackCommand = new DelegateCommand((_) => {
                OnBackEvent("Menu");
            });
        }

        private void OnBackEvent(String page) {
            BackEvent?.Invoke(this, (page, Level ?? "", false));
        }
    }
}
