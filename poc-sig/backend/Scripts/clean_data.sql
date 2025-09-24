-- Script pour nettoyer toutes les données de la base POC SIG
-- Supprime toutes les features et layers

-- Désactiver temporairement les contraintes de clés étrangères
ALTER TABLE Features NOCHECK CONSTRAINT ALL;

-- Supprimer toutes les features
DELETE FROM Features;
DBCC CHECKIDENT ('Features', RESEED, 0);

-- Supprimer toutes les layers
DELETE FROM Layers;
DBCC CHECKIDENT ('Layers', RESEED, 0);

-- Réactiver les contraintes
ALTER TABLE Features CHECK CONSTRAINT ALL;

PRINT 'Base de données nettoyée avec succès';
PRINT 'Nombre de features: ' + CAST((SELECT COUNT(*) FROM Features) AS VARCHAR);
PRINT 'Nombre de layers: ' + CAST((SELECT COUNT(*) FROM Layers) AS VARCHAR);