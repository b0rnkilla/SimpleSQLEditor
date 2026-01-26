# SimpleSQLEditor – Roadmap & Lernziele

Dieses Projekt ist ein Lern- und Referenzprojekt mit dem Ziel,
SQL Server Administration und Entity Framework Core bewusst zu verstehen
und vergleichen zu können (kein Blindflug).

---

## 🔹 Kurzfristiger Plan (konkret & versioniert)

### v0.7.8 – PK / FK Anzeige (read-only)
Ziel: Tabellen-Schema besser verstehen

- [x] Primary Key (PK) Spalten anzeigen
- [x] Foreign Key (FK) Spalten anzeigen
- [x] Anzeige in der Columns-Liste (z.B. `[PK]`, `[FK]`)
- [x] Keine Bearbeitung, nur Lesen

---

### v0.8.x – Entity Framework Core Einstieg

#### v0.8.0 – EF Core Setup
Ziel: EF Core technisch einführen

- [x] EF Core NuGet Packages
- [x] DbContext einführen
- [x] ConnectionString aus UI verwenden (Runtime)
- [x] Kein Code-First, keine Migration
- [x] Erste einfache Abfrage (sys.databases)
- [x] EF Raw SQL Kompositions-Fallen verstanden und behoben (ORDER BY / Semikolon)

#### v0.8.1 – SQL/EF Umschaltung vorbereiten (Architektur)
Ziel: Vergleichbar und verständlich zwischen SQL und EF umschalten können

- [x] Globalen Modus einführen: `DataAccessMode = Sql | Ef`
- [x] UI-Switch vor Connect (Modus auswählbar)
- [x] Fassade für Datenbankkatalog eingeführt (Start: Datenbanken)
- [x] Zentrales Routing SQL / EF für Datenbanklisten
- [x] Logging/Status zeigt technische Quelle pro Operation (SQL / EF)
      (Quelle wird dynamisch pro Operation bestimmt, nicht global)

Hinweis:
EF erscheint im Log nur dort, wo EF technisch tatsächlich verwendet wird.

#### v0.8.2 – Provider-Logging und TableData-Statusmeldungen
Ziel: Provider pro Operation sichtbar machen und TableData in den Log integrieren

- [x] Logging/Status um Provider-Präfix erweitern (OperationSource-Kontext)
- [x] Begin-Scopes für Operationen einführen (SQL vs. geroutet)
- [x] TableData: Statusmeldungen „Loading rows…“ / „Loaded X rows.“ ergänzen
- [x] DataAccess Mode UI (SQL/EF) vor Connect verfügbar

Hinweis:
Diese Version enthält bewusst umfangreiche Architektur- und Struktur-Refactorings zur Vorbereitung weiterer EF-Lernschritte.

#### v0.8.3 – Zentrale DataAccess-Fassade + EF-Read (Databases/Tables/Columns/TableData)
Ziel: Read-Operationen zwischen SQL und EF im selben UI-Flow vergleichbar machen (ohne Entities)

- [x] Zentrale Fassade einführen: IDataAccessService (Read-UseCases)
- [x] Zentraler Router: DataAccessRouterService (Mode-basiert)
- [x] SQL-Implementierung über SqlDataAccessService angebunden
- [x] EF-Implementierung über EfDataAccessService/EfDatabaseAdminService angebunden
- [x] Datenbankenliste per EF lesen (sys.databases)
- [x] Tabellenliste per EF lesen (sys.tables)
- [x] Spalten + Datentypen per EF lesen (sys.columns / sys.types)
- [x] TableData (Rows) per EF lesen (ohne Entities)
- [x] Services-Ordner strukturiert: DataAccess, Sql, EfCore, State, Ui
- [x] ViewModels: Read-Pfade auf IDataAccessService umgestellt (Main + TableData)
- [x] Logging vereinheitlicht: BeginSqlOperation und BeginRoutedOperation

Hinweis:
Diese Version enthält bewusst umfangreiche Architektur- und Struktur-Refactorings zur Vorbereitung weiterer EF-Lernschritte.

#### v0.8.4 – PK/FK Read per EF
Ziel: Keys auch im EF-Read-Pfad sichtbar machen (Schema-Vergleich vervollständigen)

- [x] Primary Key Spalten per EF lesen
- [x] Foreign Key Spalten per EF lesen
- [x] Anzeige der Tags [PK]/[FK] weiterhin identisch im Columns-UI (SQL vs. EF)

#### v0.8.5 – Service-Struktur konsolidieren (Refactoring)
Ziel: Klarere Vergleichbarkeit zwischen SQL- und EF-Pfaden vor v0.9.x

- [x] Services-Ordner logisch konsolidieren (Sql / EfCore / DataAccess)
- [x] EF RuntimeContextFactory entfernt und DbContext-Erzeugung internalisiert
- [x] DatabaseNameRow in EfDbContext integriert
- [x] Kleinstdateien im EF-Bereich reduziert
- [x] Keine Verhaltensänderung (reines Refactoring)
- [x] Architektur- und Lernziele unverändert beibehalten

#### v0.8.6 – TestConnection über gerouteten DataAccess-Pfad
Ziel: Verbindungstest bewusst zwischen SQL und EF vergleichen

- [x] Neuen Read-/Infra-UseCase einführen: TestConnection
- [x] TestConnection in IDataAccessService aufnehmen
- [x] Routing SQL / EF über DataAccessRouterService
- [x] SQL-Implementierung: SqlConnection.OpenAsync (bestehendes Verhalten)
- [x] EF-Implementierung: OpenConnectionAsync / CloseConnectionAsync
- [x] Status-/Log-Ausgabe zeigt verwendeten Provider (SQL / EF)
- [x] Connect-Command nutzt den gerouteten TestConnection-Pfad
- [x] Vergleich: Was macht EF beim „Connect“, was nicht?

---

### v0.9.x – Daten bearbeiten (EF Core)

#### v0.9.0 – Row Selection
- [x] Zeilen im DataGrid auswählen
- [x] Primary Key erkennen (geroutet: SQL / EF)
- [x] Anzeige der Row-Details
- [x] PK visuell hervorgehoben
- [x] UI Skalierbar bei vielen Spalten (GridSplitter + ScrollViewer)

#### v0.9.1 – EF Change Tracking verstehen (Read → Track)

Ziel: Verstehen, wie EF Core Daten verfolgt, bevor Änderungen geschrieben werden

- [x] Vergleich: SQL DataTable vs. EF-tracked Daten (UI zeigt DataTable; EF-Tracking läuft separat über tracked Session)
- [x] Laden einer Zeile über EF mit Tracking (dynamisches Tracking per SharedTypeEntity ohne Entities)
- [x] ChangeTracker States analysieren (Unchanged / Modified) (Header-Ausgabe + Demo-Buttons)
- [x] Änderungen im UI vornehmen, ohne zu speichern (Demo: Simulate Change / Revert ohne SaveChanges)
- [x] Sichtbar-machen, wann EF Änderungen erkennt (Header: Modified + Modified Columns)
- [x] Anzeige des Tracking-Zustands im Header des TableDataWindows sichtbar machen
- [x] Abgrenzung: UI-State vs. EF-State (UI bleibt read-only; EF-State separat sichtbar)

Hinweis:
In dieser Version erfolgt **noch kein Schreiben in die Datenbank**.
Der Fokus liegt ausschließlich auf dem Verständnis von Tracking und State-Übergängen.

#### v0.9.2 – Verbindungs- & Fenster-Lifecycle (Stabilität & UX)

- [ ] „Disconnect“-Button im MainWindow
- [ ] Disconnect setzt den kompletten App-State zurück
  - IsConnected = false
  - Auswahl von Database / Table / Column zurücksetzen
  - Buttons & ComboBoxen wieder sperren
- [ ] Disconnect schließt alle geöffneten Nebenfenster
  - TableDataWindow
  - StatusLogWindow
  - SqlDataTypesWindow
- [ ] Während TableDataWindow geöffnet ist:
  - Sperren der ComboBoxen (Database / Table / Column)
  - Sperren aller zugehörigen Aktionen (Load, Create, Delete, etc.)
- [ ] Sicherstellen, dass beim Schließen von Fenstern:
  - laufende EF-Tracking-Sessions deterministisch disposed werden
  - DbContexts zuverlässig freigegeben werden
  - keine UI- oder Event-Referenzen „hängen bleiben“
- [ ] Ziel:
  - klarer, deterministischer Lebenszyklus  
    **Connect → Arbeiten → Disconnect → garantiert sauberer Zustand**
  - kein implizites Verlassen auf den GC

#### v0.9.3 – Update einzelner Werte (EF Core)

- [ ] Einzelne Spalten im Row-Details-Bereich editierbar machen
- [ ] Änderungen über EF Core Change Tracking erkennen
- [ ] SaveChanges bewusst und explizit auslösen
- [ ] Cancel / Revert von Änderungen
  - EF-Entity-State zurücksetzen
  - kein Datenbankzugriff beim Cancel
- [ ] Transaktionen verstehen
  - wann EF implizit Transaktionen nutzt
  - wann explizite Transaktionen sinnvoll / notwendig sind
- [ ] Generiertes SQL analysieren
  - UPDATE-Statements
  - Parameter
  - Transaktionsgrenzen

#### v0.9.4 – Cancellation & Abbruchlogik (gezielt & bewusst)

- [ ] CancellationToken-Basis einführen
  - optionales CancellationToken in DataAccess-Methoden
  - Durchreichen über Router → SQL / EF
- [ ] Abbruch bei Disconnect
  - laufende DB-Operationen kontrolliert abbrechen
  - Cancel → Await → Dispose als feste Abfolge
- [ ] Optional: Cancel-Button bei langlaufenden Operationen
  - z.B. Reload TableData
  - explizit *kein* Zwang für jeden Vorgang
- [ ] Bewusster Umgang mit OperationCanceledException
  - kein „Error“-Status
  - normaler Abbruchpfad
- [ ] Ziel:
  - **kontrollierter Abbruch laufender Arbeit**
  - Cancellation = *Abbruch-Signal*
  - Dispose = *deterministisches Aufräumen*
  - keine Deadlocks, keine hängenden Threads, keine Zombie-Tasks

#### v0.9.x (vor v1.0) – Dokumentation im Code

- [ ] XML-Summaries an allen relevanten öffentlichen Klassen & Methoden
- [ ] Kurze, gezielte Kommentare an komplexen Stellen
  - nicht „WIE“, sondern „WARUM“ und ggf. „WAS“
- [ ] Fokus:
  - Nachvollziehbarkeit beim Debuggen
  - Verständnis von EF-Interna & Lifecycle

Hinweis / Merksatz:  
> **„Code sagt *wie*, Kommentare sagen *warum*.“**

#### v0.9.x (vor v1.0) – Refactoring & Speicherhygiene

- [ ] Event-Subscriptions sauber verwalten
  - keine anonymen Lambdas ohne Abmeldung
  - Dispose-Pattern bei Services & ViewModels mit Events
- [ ] Sauberer Umgang mit Ressourcen
  - DbContext, Connections, Reader, Tracking-Sessions
  - keine implizite Abhängigkeit auf den Garbage Collector
- [ ] Vermeiden von schleichenden Speicherlecks
  - Window-Lifecycle prüfen
  - ViewModel-Referenzen lösen
- [ ] Ziel:
  - stabiler RAM-Verbrauch über lange Laufzeit
  - reproduzierbares, deterministisches Shutdown-Verhalten

---

## 🔹 Schema / DDL (später oder optional)

- [ ] NULL / NOT NULL support
- [ ] NULLability anzeigen
- [ ] NULLability ändern (ALTER TABLE)
- [ ] Primary Key setzen / entfernen
- [ ] Composite Primary Keys
- [ ] Foreign Keys anlegen / entfernen
- [ ] Referenzierte Tabellen und Spalten auswählen
- [ ] ON DELETE / ON UPDATE Regeln

---

## 🔹 Daten (nicht DDL)

- [ ] Zeilen einfügen
- [ ] Zeilen bearbeiten
- [ ] Zeilen löschen
- [ ] Validierung auf UI- und DB-Ebene

---

## 🔹 Entity Framework Core – Lernziele

- [ ] DbContext Lebenszyklus verstehen
- [ ] Tracking vs. No-Tracking
- [ ] ChangeTracker analysieren
- [ ] SQL-Generierung nachvollziehen
- [ ] EF vs. manuelles SQL abwägen

---

## 🔹 UX / Architektur

- [ ] Reusable Dialog Services
- [ ] Konsistentes Error Handling
- [ ] Validation Feedback verbessern
- [ ] Trennung: SQL-Admin vs. EF-Funktionen
