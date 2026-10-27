# Edytor wielokątów – dokumentacja

To okno można w każdej chwili otworzyć przyciskiem „Help”.

## 1. Instrukcja obsługi

### Mysz

- Lewy przycisk + przeciąganie wierzchołka: przesuwa wierzchołek. Ograniczenia i ciągłość
  są zachowywane; w razie potrzeby przesuwają się inne wierzchołki i punkty kontrolne.
  Jeśli dla nowej pozycji ograniczeń nie da się spełnić, przesuwany jest cały wielokąt,
  więc wierzchołek zawsze zostaje pod kursorem.
- Lewy przycisk + przeciąganie punktu kontrolnego Béziera (niebieski punkt): przesuwa punkt
  kontrolny. Wierzchołki jego własnego segmentu stoją w miejscu; sąsiedni odcinek może się
  obrócić lub wydłużyć, żeby zachować ciągłość G1/C1.
- Lewy przycisk + przeciąganie krawędzi: przesuwa cały wielokąt.
- Prawy przycisk na krawędzi: menu krawędzi.
    - Add vertex – wstawia wierzchołek w środku krawędzi.
    - Set length… – ograniczenie stałej długości (tylko odcinki). Okno dialogowe podpowiada
      aktualną długość.
    - Make parallel… – potem kliknij lewym przyciskiem drugą krawędź (w dowolnym wielokącie),
      żeby obie krawędzie były równoległe. Na czas wyboru pierwsza krawędź jest podświetlona
      na pomarańczowo. Prawy przycisk anuluje wybór. Dostępne tylko dla odcinków.
    - Convert to line / Convert to Bézier / Convert to arc – zmienia kształt krawędzi.
    - Remove L=… / Remove ∥n – usuwa ograniczenie tej krawędzi.
- Prawy przycisk na wierzchołku: menu wierzchołka.
    - Delete vertex – nieaktywne, gdy wielokąt ma tylko 3 wierzchołki.
    - Continuity G0 / G1 / C1 – widoczne tylko, gdy wierzchołek styka się z segmentem Béziera
      lub łukiem. Aktualna wartość jest oznaczona ✓. Pokazywane są tylko klasy ciągłości
      możliwe w danym wierzchołku (patrz 2).

### Klawiatura

- Edytor obsługuje się wyłącznie myszą; nie ma skrótów klawiszowych.
  W oknie długości Enter zatwierdza, a Esc anuluje.

### Pasek narzędzi

- Library Algorithm / Bresenham Algorithm – wybór sposobu rysowania odcinków
  (DrawLine z Avalonii albo moja implementacja algorytmu Bresenhama do bufora pikseli).
  Krzywe Béziera i łuki są rysowane jako wiele krótkich odcinków, więc też używają wybranego
  algorytmu. Elementy pomocnicze (wierzchołki, wielokąt kontrolny, etykiety, pomarańczowe
  podświetlenie) są zawsze rysowane biblioteką.

### Co widać na scenie

- Czarne punkty – wierzchołki.
- Niebieskie punkty i niebieskie przerywane linie – punkty kontrolne i wielokąt kontrolny Béziera.
- Niebieskie etykiety przy wierzchołkach – klasa ciągłości (tylko przy wierzchołkach obok krzywej).
- Czerwone etykiety przy środku krawędzi – ograniczenia: `L=120` to stała długość,
  `∥1`, `∥2`, … oznaczają pary równoległe (obie krawędzie pary mają ten sam numer).

## 2. Założenia

- Ograniczenia (stała długość, równoległość) są zdefiniowane tylko dla odcinków.
  Zamiana krawędzi na segment Béziera lub łuk usuwa jej ograniczenia.
- Dwóch krawędzi ze wspólnym wierzchołkiem nie można uczynić równoległymi (musiałyby leżeć
  na jednej prostej).
- Dodanie lub usunięcie wierzchołka usuwa ograniczenia krawędzi, które go dotyczą.
  Nowe krawędzie są zawsze odcinkami, nawet jeśli stara krawędź była krzywą.
- Nowe ograniczenie, którego nie da się spełnić (np. jest sprzeczne z innymi), nie jest
  dodawane. Przeciągnięcie wierzchołka, którego nie da się spełnić, przesuwa cały wielokąt.
- Domyślna ciągłość to G1. Gdy krawędź staje się segmentem Béziera, jej
  wierzchołki dostają G1, chyba że użytkownik wcześniej sam wybrał ich ciągłość.
- Łuki obsługują tylko G0 i G1. Gdy krawędź staje się łukiem, C1 na jej końcach zmienia się
  na G1, a końce bez jawnego wyboru dostają G0.
- Tylko jeden koniec łuku może mieć G1. Wybranie G1 na drugim końcu przełącza pierwszy na G0.
- W wierzchołku między dwoma łukami możliwe jest tylko G0 (żaden łuk nie może dopasować się
  do drugiego).
- Łuk nie ma punktów kontrolnych: jego okrąg jest liczony z wierzchołków i sąsiedniej
  krawędzi (patrz 4.4), więc po prostu podąża za sąsiadami.
- Przeciąganie punktu kontrolnego nigdy nie przesuwa wierzchołków jego własnego segmentu.
  Przy G1/C1 może przesunąć dalszy wierzchołek sąsiedniego odcinka; jeśli ten wierzchołek
  jest trzymany przez ograniczenia, sąsiad zostaje bez zmian.

## 3. Model danych

- `Vec2` – niemutowalny wektor 2D ze zwykłymi operatorami, `Dot`, `Cross`, `Length`, `Normalized`.
- `Scene` – lista wielokątów (`Polygon`) i lista ograniczeń (`Constraint`). Ograniczenia
  należą do sceny, a nie do wielokąta, bo równoległość może łączyć krawędzie dwóch różnych
  wielokątów. Operacje: `TryAddConstraint` i `SetFixedLength` (ograniczenie niemożliwe do spełnienia jest
  wycofywane, a stara długość krawędzi wraca) oraz `SplitEdge`/`RemoveVertex`, które usuwają
  ograniczenia zmienionych krawędzi. Widok wywołuje tylko te metody.
- `Polygon` – uporządkowane listy `Vertex` i `Edge`; krawędź i prowadzi od wierzchołka i
  do i+1 (ostatnia krawędź zamyka wielokąt). Samoprzecięcia są dozwolone.
  Operacje: `Translate`, `SplitEdge`, `RemoveVertex`, `EdgeBefore`, `EdgeAfter`.
  Reguły ciągłości w wierzchołku: `IsCurvedAt` (czy styka się tu krzywa),
  `AllowsContinuity` (czy dana klasa ciągłości jest tu możliwa), `KeepSingleG1`
  (pilnuje, żeby łuk miał G1 najwyżej na jednym końcu).
- `Vertex` – `Position`, `Continuity` (G0/G1/C1) i `ContinuityIsExplicit`
  (true, gdy ciągłość wybrał użytkownik albo scena demo).
- `Edge` – wskazuje wierzchołki `Start` i `End` i ma kształt `Shape`. Liczy też
  `Direction`, `Length`, `Midpoint` i wektory styczne na obu końcach (pyta o nie kształt).
- `EdgeShape` (abstrakcyjna) – jak wygląda krawędź między wierzchołkami:
    - `LineShape` – odcinek,
    - `BezierShape` – krzywa Béziera 3. stopnia z dwoma punktami kontrolnymi `Control1`,
      `Control2` (krzywa to Start, Control1, Control2, End),
    - `ArcShape` – łuk okręgu; nic nie przechowuje, okrąg liczy `ArcGeometry`.

  Zmiana kształtu krawędzi podmienia tylko ten obiekt; wierzchołki zostają te same.

  Zachowanie zależne od kształtu jest zapisane jako metody i właściwości wirtualne
  (polimorfizm), więc reszta kodu nie sprawdza typu kształtu:
    - `TangentAtStart` / `TangentAtEnd` – wektory styczne na końcach,
    - `FollowEnds` – co zrobić, gdy wierzchołki krawędzi się przesunęły (Bézier przesuwa
      swoje ramiona),
    - `Curve` – krzywa parametryczna `t ∈ [0, 1] → punkt`, używana do trafiania myszą,
    - `IsStraight` – czy to odcinek (ograniczenia, brak etykiety ciągłości),
    - `AdaptsToNeighbours` – czy kształt sam dopasowuje się do stycznej sąsiada (łuk),
    - `MaxContinuity` – najwyższa ciągłość możliwa na końcach (łuk: G1, pozostałe: C1),
    - `ApplyDefaultContinuity` – ustawia domyślną ciągłość zaraz po zmianie kształtu.

  `LineShape`, `BezierShape` i `ArcShape` nadpisują tylko to, co u nich jest inne.
  Sprawdzanie typu zostało tylko tam, gdzie kod potrzebuje danych jednego typu (punkty
  kontrolne Béziera), oraz przy rysowaniu w widoku, bo model nie zależy od kodu rysującego.
- `Constraint` (abstrakcyjna) – `Edges`, `IsSatisfied()`, `Project(pinned)`, `GetLabel()`.
  Implementacje: `FixedLengthConstraint(edge, length)`, `ParallelConstraint(first, second)`.
- `DemoScene` – buduje scenę startową; dodanie wielokąta to kilka linii (utworzenie z punktów,
  dodanie do `scene.Polygons`, potem ustawienie kształtów, ciągłości i ograniczeń).

Układ kodu: `Model` (dane), `Geometry` (typy matematyczne), `Constraints` (solvery),
`Rendering` (algorytmy rysowania), `Editing` (trafianie myszą), `Views` (interfejs).
Model nie zależy od interfejsu.

## 4. Algorytmy

### 4.1 Solver ograniczeń (`ConstraintSolver`)

Iteracyjne rzutowanie (relaksacja):

1. Zapamiętaj pozycje wszystkich wierzchołków.
2. Powtarzaj najwyżej 100 razy: jeśli każde ograniczenie jest spełnione (tolerancja 1e-6),
   zakończ sukcesem. W przeciwnym razie wywołaj po kolei `Project` każdego ograniczenia.
   `Project` przesuwa wierzchołki swoich krawędzi jak najmniej, żeby to jedno ograniczenie
   było spełnione; może przy tym zepsuć inne, które zostanie poprawione w następnym przebiegu.
3. Jeśli ograniczenia dalej nie są spełnione, przywróć zapamiętane pozycje i zgłoś porażkę.

Przypięty wierzchołek to ten, który przeciąga użytkownik. Ograniczenia nigdy go nie
przesuwają, więc zostaje pod kursorem; zamiast niego przesuwa się drugi koniec krawędzi.

- Stała długość: błąd `|AB| - L` jest poprawiany wzdłuż kierunku krawędzi – w całości na
  wolnym końcu, jeśli drugi jest przypięty, w przeciwnym razie po połowie na każdym końcu.
- Równoległość: docelowy kierunek to kierunek krawędzi z przypiętym wierzchołkiem albo,
  jeśli żaden nie jest przypięty, średnia obu kierunków jednostkowych (drugi kierunek jest
  najpierw odwracany, jeśli krawędzie są skierowane przeciwnie, żeby nie musiały się obracać
  o 180°). Każda krawędź jest potem obracana do docelowego kierunku z zachowaniem długości,
  wokół przypiętego wierzchołka albo wokół swojego środka.

Przesunięcie wierzchołka = ustaw nową pozycję i uruchom solver; jeśli się nie uda, stara
pozycja jest przywracana, a zamiast tego przesuwany jest cały wielokąt.
Dodanie ograniczenia = dodaj je i uruchom solver; jeśli się nie uda, ograniczenie jest usuwane.

### 4.2 Ciągłość (`ContinuitySolver`)

Wektory styczne na końcach krawędzi (`EdgeShape.TangentAtStart` / `TangentAtEnd`): odcinek ma
`B - A` na obu końcach; segment Béziera ma `3(P1 - P0)` na początku i `3(P3 - P2)` na końcu.

- `Apply` – dla każdego wierzchołka z G1 lub C1 obok segmentu Béziera punkt kontrolny przy
  wierzchołku jest ustawiany na prostej stycznej drugiej krawędzi:
    - G1: ten sam kierunek, ramię zachowuje obecną długość,
    - C1: ten sam wektor styczny, więc ramię to `styczna / 3`.

  Wierzchołki nie są tu przesuwane, więc ograniczenia krawędzi pozostają spełnione.
  Przeciągany punkt kontrolny jest zablokowany; dopasowywany jest ten po drugiej stronie.
  Wierzchołki obok łuku są pomijane – łuk sam dopasowuje się do sąsiada (patrz 4.4).
- Po przeciągnięciu wierzchołka punkty kontrolne przy każdym przesuniętym wierzchołku są
  przesuwane o ten sam wektor (`FollowVertices`, które wywołuje `EdgeShape.FollowEnds`),
  potem `Apply` przywraca ciągłość.
- Przeciąganie punktu kontrolnego obok odcinka z G1/C1: dalszy wierzchołek tego odcinka jest
  przesuwany (przez solver ograniczeń) tak, żeby odcinek leżał na przedłużeniu ramienia
  kontrolnego. Dla C1 długość odcinka staje się równa 3 × długość ramienia.

### 4.3 Rysowanie krzywej Béziera (`BezierDrawer`) – algorytm przyrostowy

1. Przejście do bazy potęgowej: `P(t) = A t³ + B t² + C t + D`, gdzie
   `A = -P0 + 3P1 - 3P2 + P3`, `B = 3P0 - 6P1 + 3P2`, `C = 3(P1 - P0)`, `D = P0`.
2. Liczba kroków n zależy od długości wielokąta kontrolnego (ok. 5 px na krok), krok h = 1/n.
3. Różnice w przód: `Δ1 = A h³ + B h² + C h`, `Δ2 = 6A h³ + 2B h²`, `Δ3 = 6A h³`.
4. Zaczynając od `P0`, każdy kolejny punkt to `punkt + Δ1`, potem `Δ1 += Δ2`, `Δ2 += Δ3` –
   na punkt przypadają same dodawania. Kolejne punkty są łączone wybranym algorytmem rysowania
   odcinków; ostatni punkt jest ustawiany dokładnie na `P3`.

### 4.4 Łuki (`ArcGeometry`, `ArcDrawer`)

- G1 na początku A (sąsiednia krawędź nie jest łukiem): okrąg przechodzi przez A i B i jest
  w A styczny do stycznej t poprzedniej krawędzi. Środek leży na normalnej n w A:
  `C = A + r·n`, ze znakowanym promieniem `r = |B - A|² / (2 n·(B - A))`. Kierunek łuku to ten,
  w którym łuk wychodzi z A wzdłuż t. G1 na końcu B to ta sama konstrukcja, liczona od B wstecz.
  Jeśli B leży na prostej stycznej, łuk się degeneruje i rysowany jest odcinek.
- G0 na obu końcach: półokrąg o środku w połowie krawędzi i promieniu |AB| / 2.
- Rysowanie: łuk jest dzielony na kawałki długości ok. 5 px; punkty `C + r(cos φ, sin φ)`
  są łączone wybranym algorytmem rysowania odcinków.

### 4.5 Odcinek Bresenhama (`BresenhamLineDrawer`)

Algorytm punktu środkowego z wykładu (`MidpointLine`), rozszerzony na wszystkie oktanty.
Dla linii łagodnej (`|dx| ≥ |dy|`) x zmienia się w każdym kroku, a o y decyduje znak
`d = 2·F(M)`, gdzie M to środek między kandydatami E i NE: start `d = 2dy - dx`,
po E `d += incrE = 2dy`, po NE `d += incrNE = 2(dy - dx)` i y przesuwa się o 1.
Liczymy na `|dx|`, `|dy|`, a kierunek kroku dają `sx`, `sy` = ±1. Linia stroma
(`|dy| > |dx|`) to ta sama konstrukcja z zamienionymi rolami x i y. Same liczby
całkowite, jedno porównanie i jedno dodawanie na piksel. Piksele trafiają do
`PixelBuffer`, który jest kopiowany do bitmapy.

### 4.6 Trafianie myszą (`HitTester`)

Kolejność: wierzchołek (promień 8 px), potem punkt kontrolny, potem krawędź (5 px).
Każdy kształt zwraca swoją krzywą parametryczną (`EdgeShape.Curve`); krzywa jest przybliżana
łamaną z 32 odcinków, a odległość od punktu to najmniejsza odległość od tych odcinków.
Dla odcinka daje to dokładnie odległość od odcinka.
