# 3Dinoz Game — Case Study

## English

A Unity color-matching puzzle: move rings hanging on chains onto same-colored sticks until every stick is filled.

### Project overview

- **Engine:** Unity **2022.3.62f2 LTS** (URP — Universal Render Pipeline).
- **Summary:** Level data loads from JSON under `Resources/LevelDatas`. Each **stick** has a fixed color and accepts up to **3 rings**. **Chain** ring colors come from level design; you win when all sticks are filled, and you lose when time runs out.
- **Architecture:** Signal-based flow for game and UI; `LevelGenerator` builds levels; input and chain logic live in `ChainController` / `RingHandler`.

### How to play

1. **Click the chain.** One of the bottom “leaf” rings moves with animation to the **nearest stick** that matches its color and still has space.
2. **Color must match.** If the ring color does not match any available stick, no move happens (audio feedback).
3. **Win:** When **all sticks** are filled with **three rings** each, the level ends (win panel and sound).
4. **Lose:** When the on-screen **countdown** reaches zero, the game ends (fail panel).

Progression and scene flow use `GameManager`; for testing you can pin a level index on `LevelGenerator`.

### How to use the level editor

Editor window: menu **3Dinoz → LevelGeneratorEditor**.

1. **Level data:** Files `Level_{n}.json` under `Assets/Resources/LevelDatas`. Pick a number with **Level slot**; **Save** / **Load** to write or read. **Add new & Save** switches to a new slot and saves; **Refresh list** rescans files.
2. **Generator:** Set **Stick count** (1–5) and **Hook count** (1–3). **Timer (seconds)** is written into the JSON for that level.
3. **Brush:** Choose a color from the palette; in **Hook chains**, **click-drag** to paint rings (trunk and optional branches).
4. **Stick preview:** Set stick colors here; layout should stay consistent with in-game stick placement.
5. **Level validation:** Checks total ring count and per-color capacity rules (e.g. sticks × 3); fix any warnings.

At runtime, the level loads from `Level_{n}.json` using saved progress index or the optional test index from the editor.

### External assets / AI tools

- **DOTween** (Demigiant) — animation and tweening (`DG.Tweening`).
- **TextMesh Pro** — Unity package; UI text.
- **Casual Game Sounds U6** — sound pack.
- **JMO Assets** — particle pack.

**Cursor AI** (development assistance)

- Faster coding and boilerplate generation  
- Small refactors and suggestions that speed up development  

---

## Türkçe

Unity tabanlı renk eşleştirme bulmacası: zincirlerde asılı halkaları, aynı renkteki çubuklara taşıyarak tüm çubukları doldurmayı hedeflersiniz.

### Proje özeti

- **Motor:** Unity **2022.3.62f2 LTS** (URP — Universal Render Pipeline).
- **Özet:** Seviye verisi JSON olarak `Resources/LevelDatas` altından yüklenir. Her **çubuk (stick)** sabit bir renge sahiptir ve en fazla **3 halka** kabul eder. **Zincirlerdeki** halkaların renkleri level tasarımına göre belirlenir; tüm çubuklar dolduğunda seviye kazanılır, süre biterse kaybedilir.
- **Mimari:** Oyun akışı ve UI için sinyal tabanlı iletişim; seviye üretimi `LevelGenerator`, giriş ve zincir mantığı `ChainController` / `RingHandler` ile yönetilir.

### Nasıl oynanır

1. **Zincire tıklayın.** En alttaki halkalardan biri, aynı renkte ve hâlâ yer olan **en yakın çubuğa** animasyonla taşınır.
2. **Renk eşleşmesi şarttır.** Halkanın rengi, çubuğun rengiyle aynı değilse veya uygun çubuk yoksa eşleşme gerçekleşmez (ilgili ses geri bildirimi verilir).
3. **Kazanma:** Tüm çubuklar **üçer halkayla** dolduğunda seviye biter (kazanma paneli ve ses).
4. **Kaybetme:** Ekrandaki **geri sayım süresi** bittiğinde oyun biter (kaybetme paneli).

İlerleme ve sahne geçişleri `GameManager` ile; test için `LevelGenerator` üzerinde isteğe bağlı sabit level indeksi kullanılabilir.

### Level editörü

Editör penceresi: menü çubuğunda **3Dinoz → LevelGeneratorEditor**.

1. **Level data:** `Assets/Resources/LevelDatas` altında `Level_{n}.json` dosyaları. **Level slot** ile numara seçin; **Save** / **Load** ile kaydedin veya yükleyin. **Add new & Save** yeni slot numarasına geçip kaydeder; **Refresh list** dosya listesini yeniler.
2. **Generator:** **Stick count** (1–5) ve **Hook count** (1–3) ayarlayın. **Timer (seconds)** bu level için süreyi JSON’a yazar.
3. **Fırça:** Paletten renk seçin; **Hook chains** bölümünde gövde ve isteğe bağlı dallarda halkaları **tıklayıp sürükleyerek** boyayın.
4. **Stick önizlemesi:** Çubukların renklerini buradan düzenleyin; oyun içi çubuk yerleşimi ile uyumlu olmalıdır.
5. **Level doğrulama:** Toplam halka sayısı ve renk başına kapasite kuralları kontrol edilir; uyarıları düzeltin.

Oynanışta level, kayıtlı ilerleme indeksine veya editörde tanımlı test indeksine göre `Level_{n}.json` dosyasından yüklenir.

### Harici varlıklar / yapay zeka araçları

- **DOTween** (Demigiant) — animasyon ve tween (`DG.Tweening`).
- **TextMesh Pro** — Unity paketi; UI metinleri.
- **Casual Game Sounds U6** — ses paketi.
- **JMO Assets** — partikül paketi.

**Cursor AI** (geliştirme yardımı)

- Hızlı kod yazımı ve boilerplate üretimi  
- Küçük refactor işlemleri ve geliştirmeyi hızlandıran öneriler