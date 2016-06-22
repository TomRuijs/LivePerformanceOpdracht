--DROP TABLES
DROP TABLE BOOT CASCADE CONSTRAINTS;
DROP TABLE MEER CASCADE CONSTRAINTS;
DROP TABLE GEBRUIKER CASCADE CONSTRAINTS;
DROP TABLE ARTIKEL CASCADE CONSTRAINTS;
DROP TABLE HUURCONTRACT CASCADE CONSTRAINTS;
DROP TABLE MEERHUUR CASCADE CONSTRAINTS;
DROP TABLE ARTIKELHUUR CASCADE CONSTRAINTS;
DROP TABLE BOOTHUUR CASCADE CONSTRAINTS;
DROP TABLE MOTORBOOT CASCADE CONSTRAINTS;

CREATE TABLE BOOT
(
  BootId NUMBER(10) PRIMARY KEY,
  Naam VARCHAR2(255),
  Prijs NUMBER(10, 2),
  Soortboot VARCHAR2(255)
);

CREATE TABLE MEER
(
  MeerId NUMBER(10) PRIMARY KEY,
  Naam VARCHAR2(255),
  Prijs NUMBER(10, 2)
);

CREATE TABLE GEBRUIKER
(
  GebruikerId NUMBER(10),
  Email VARCHAR2(255),
  Naam VARCHAR2(64),
  Wachtwoord VARCHAR2(64)
);

CREATE TABLE HUURCONTRACT
(
  HuurcontractId NUMBER(10) PRIMARY KEY,
  DatumVan DATE,
  DatumTot DATE,
  HuurderId NUMBER(10)
);

CREATE TABLE ARTIKEL 
(
  ArtikelId NUMBER(10),
  Prijs NUMBER(10, 2),
  Naam VARCHAR2(255)
);

CREATE TABLE ARTIKELHUUR
(
  ArtikelId NUMBER(10),
  HuurcontractId NUMBER(10),
  PRIMARY KEY (ArtikelId, HuurcontractId)
);

CREATE TABLE MOTORBOOT
(
  SoortBoot VARCHAR2(255) PRIMARY KEY,
  Tankinhoud NUMBER(10,2)
);

CREATE TABLE MEERHUUR
(
  HuurcontractId NUMBER(10),
  MeerId NUMBER(10),
  PRIMARY KEY (MeerId, HuurcontractId)
);

CREATE TABLE BOOTHUUR
(
  BootId NUMBER(10),
  HuurcontractId NUMBER(10),
  PRIMARY KEY (BootId, HuurcontractId)
);

--FOREIGN KEY CONSTRAINTS
ALTER TABLE BOOTHUUR ADD FOREIGN KEY (BootId) REFERENCES BOOT(BootId);
ALTER TABLE BOOTHUUR ADD FOREIGN KEY (HuurcontractId) REFERENCES HUURCONTRACT(HuurcontractId);
ALTER TABLE MEERHUUR ADD FOREIGN KEY (HuurcontractId) REFERENCES HUURCONTRACT(HuurcontractId);
ALTER TABLE MEERHUUR ADD FOREIGN KEY (MeerId) REFERENCES MEER(MeerId);
ALTER TABLE ARTIKELHUUR ADD FOREIGN KEY (ArtikelId) REFERENCES ARTIKEL(ArtikelId);
ALTER TABLE ARTIKELHUUR ADD FOREIGN KEY (HuurcontractId) REFERENCES HUURCONTRACT(HuurcontractId);
ALTER TABLE MOTORBOOT ADD FOREIGN KEY (BootId) REFERENCE S