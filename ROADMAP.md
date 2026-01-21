# SimpleSQLEditor – Roadmap & Lernziele

Dieses Projekt ist ein Lern- und Referenzprojekt mit dem Ziel,
SQL Server Administration und Entity Framework Core bewusst zu verstehen
und vergleichen zu können (kein Blindflug).

---

## 🔹 Kurzfristiger Plan (konkret & versioniert)

### v0.7.8 – PK / FK Anzeige (read-only)
Ziel: Tabellen-Schema besser verstehen

- [ ] Primary Key (PK) Spalten anzeigen
- [ ] Foreign Key (FK) Spalten anzeigen
- [ ] Anzeige in der Columns-Liste (z.B. `[PK]`, `[FK]`)
- [ ] Keine Bearbeitung, nur Lesen

---

### v0.8.x – Entity Framework Core Einstieg

#### v0.8.1 – EF Core Setup
Ziel: EF Core technisch einführen

- [ ] EF Core NuGet Packages
- [ ] DbContext einführen
- [ ] ConnectionString aus UI verwenden
- [ ] Kein Code-First, keine Migration
- [ ] Erste einfache Abfrage

#### v0.8.2 – Tabellen dynamisch mit EF lesen
Ziel: EF ohne feste Entities verstehen

- [ ] Tabellen ohne feste Entity-Klassen lesen
- [ ] Nutzung von `FromSqlRaw` / `Database.SqlQuery`
- [ ] Vergleich: EF vs. reines SQL
- [ ] Beobachten des erzeugten SQL

---

### v0.9.x – Daten bearbeiten (EF Core)

#### v0.9.1 – Row Selection
- [ ] Zeilen im DataGrid auswählen
- [ ] Primary Key erkennen
- [ ] Anzeige der Row-Details

#### v0.9.2 – Update einzelner Werte
- [ ] Einzelne Spalten bearbeiten
- [ ] Update über EF Core
- [ ] Validierung & Fehlerhandling

#### v0.9.3 – Change Tracking verstehen
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
