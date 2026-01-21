# SimpleSQLEditor

**SimpleSQLEditor** ist ein bewusst schlank gehaltenes Lern- und Referenzprojekt zur Arbeit mit  
**SQL Server**, **WPF (MVVM)** und **Entity Framework Core**.

Der Fokus liegt nicht auf Produktivität oder Feature-Vollständigkeit, sondern auf **Verständnis**:
- Wie funktioniert SQL Server Schema- und Datenverwaltung wirklich?
- Was übernimmt Entity Framework Core für mich – und was nicht?
- Wo liegen die Unterschiede zwischen direktem SQL und EF Core?

---

## Ziel des Projekts

Dieses Projekt wurde entwickelt, um **SQL Server und Entity Framework Core nachvollziehbar zu lernen** – nicht, um sie blind zu benutzen.

Daher gilt:
- SQL-Funktionalität wird **zuerst bewusst manuell umgesetzt**
- EF Core wird **schrittweise ergänzt**, nicht als Ersatz
- Jede Funktion soll erklärbar und nachvollziehbar sein

Das Tool dient gleichzeitig als:
- Lernprojekt
- Nachschlagewerk
- Vergleichsbasis zwischen SQL und EF Core

---

## Architektur

- **WPF (.NET)**  
- **MVVM strikt eingehalten**
- **Dependency Injection** via `Microsoft.Extensions.Hosting`
- **Keine Code-Behind-Logik für Fachfunktionen**
- **Services für UI-nahe Logik (WindowService, DialogService etc.)**

### Grundprinzipien
- ViewModels kennen **keine Windows**
- Fensterkommunikation erfolgt ausschließlich über Services
- SQL-Admin-Funktionen sind klar vom EF-Bereich getrennt
- UI-Zustände werden explizit modelliert (Busy, Error, Status)

---

## Aktueller Funktionsumfang

### 🔹 SQL Server Administration (reines SQL)
- Verbindung zu SQL Server
- Datenbanken erstellen / löschen
- Tabellen erstellen / löschen
- Spalten erstellen / löschen
- Anzeige von Datentypen inkl. Parameter (`nvarchar(50)`, `decimal(18,2)`, …)
- Anzeige von Primary Keys und Foreign Keys (read-only)
- Sicherheitsabfragen vor destruktiven Aktionen

### 🔹 Tabelleninhalte anzeigen
- Read-only Anzeige von Tabellenzeilen
- Begrenzung der Zeilenanzahl (`TOP N`)
- Klare Trennung von Schema- und Datenansicht

### 🔹 Status- & Logging-System
- Statusmeldungen mit Level (Info, Success, Warning, Error)
- Historie im separaten Fenster
- Autoscroll & Copy-Funktion
- Einheitliches UX-Verhalten

### 🔹 SQL DataTypes Window
- Übersicht erlaubter SQL Datentypen
- Kopierfunktion (ContextMenu / Strg+C)
- Unterstützung beim Definieren von Spalten

---

## Entity Framework Core – bewusst integriert

Entity Framework Core wird **nicht als Ersatz**, sondern **als Lerngegenstück** zu SQL verwendet.

### EF Core Ziele im Projekt
- DbContext verstehen
- ConnectionString zur Laufzeit verwenden
- Arbeiten **ohne Code-First**
- Keine Migrations
- Tabellen dynamisch lesen (ohne feste Entities)
- SQL-Generierung nachvollziehen
- Change Tracking verstehen

EF-Funktionalität wird parallel zu bestehendem SQL-Code aufgebaut, um Unterschiede klar zu sehen.

---

## Was dieses Projekt bewusst nicht ist

- Kein vollwertiger SQL Management Studio Ersatz
- Kein Produktiv-Tool
- Kein „Magic“-EF-Demonstrator
- Keine UI-Spielwiese ohne fachlichen Zweck

Alles, was implementiert wird, dient einem **konkreten Lernziel**.

---

## Lernschwerpunkte

- SQL Server Internals (DDL, Constraints, Keys)
- Unterschied zwischen Identity, Primary Key und Foreign Key
- MVVM sauber anwenden
- Dependency Injection in WPF
- EF Core gezielt einsetzen
- SQL vs. EF bewusst vergleichen

---

## Roadmap

Eine detaillierte, versionierte Roadmap mit geplanten Lernschritten befindet sich in der Datei: ROADMAP.md

Dort sind sowohl kurzfristige Meilensteine als auch langfristige Lernziele dokumentiert.

---

## Motivation

> „Ich will verstehen, was mein ORM für mich tut – und wann es mir im Weg steht.“

Dieses Projekt ist genau dafür gedacht.

---

## Lizenz / Nutzung

Dieses Projekt ist als **persönliches Lern- und Referenzprojekt** gedacht.  
Feel free to explore, adapt and learn from it.
