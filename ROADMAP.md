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

Hinweis:
EF erscheint im Log nur dort, wo EF technisch tatsächlich verwendet wird.

#### v0.8.2 – Tabellen dynamisch mit EF lesen
Ziel: EF ohne feste Entities verstehen

- [x] Tabellen ohne feste Entity-Klassen lesen
- [x] Nutzung von Raw SQL für Tabelleninhalte
- [x] Status-Logging für Zeilenladen inkl. Reload
- [x] Vergleichbares Verhalten SQL vs. EF im UI

#### v0.8.3 – Tabellen & Spalten dynamisch mit EF lesen
Ziel: EF-Read-Features für Katalogdaten vervollständigen (ohne Entities)

- [ ] Tabellenliste per EF lesen (sys.tables)
- [ ] Spalten + Datentypen per EF lesen (sys.columns / sys.types)
- [ ] Vergleich SQL vs. EF im selben UI-Flow (Mode Switch)

#### v0.8.4 – PK/FK Read per EF
Ziel: Keys auch im EF-Read-Pfad sichtbar machen

- [ ] Primary Key Spalten per EF lesen
- [ ] Foreign Key Spalten per EF lesen

---

### v0.9.x – Daten bearbeiten (EF Core)

#### v0.9.0 – Row Selection
- [ ] Zeilen im DataGrid auswählen
- [ ] Primary Key erkennen
- [ ] Anzeige der Row-Details

#### v0.9.1 – Update einzelner Werte
- [ ] Einzelne Spalten bearbeiten
- [ ] Update über EF Core
- [ ] Validierung & Fehlerhandling

#### v0.9.2 – Change Tracking verstehen
- [ ] Modified / Added / Deleted States
- [ ] Wann EF SQL erzeugt
- [ ] Save / Cancel Konzepte
- [ ] Transaktionen

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
