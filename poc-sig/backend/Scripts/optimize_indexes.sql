-- Create spatial indexes for better performance
-- These indexes will significantly speed up spatial queries

-- Check if spatial index already exists and create if not
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Features_Geometry' AND object_id = OBJECT_ID('Features'))
BEGIN
    CREATE SPATIAL INDEX IX_Features_Geometry
    ON Features(Geometry)
    USING GEOMETRY_GRID
    WITH (
        BOUNDING_BOX = (-180, -90, 180, 90),  -- World bounds
        GRIDS = (LOW, LOW, MEDIUM, HIGH),
        CELLS_PER_OBJECT = 16
    );
    PRINT 'Created spatial index IX_Features_Geometry'
END

-- Create index on LayerId for faster filtering
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Features_LayerId' AND object_id = OBJECT_ID('Features'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Features_LayerId
    ON Features(LayerId)
    INCLUDE (PropertiesJson, ValidFromUtc, ValidToUtc);
    PRINT 'Created index IX_Features_LayerId'
END

-- Create composite index for layer and date filtering
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Features_Layer_Dates' AND object_id = OBJECT_ID('Features'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Features_Layer_Dates
    ON Features(LayerId, ValidFromUtc, ValidToUtc);
    PRINT 'Created composite index IX_Features_Layer_Dates'
END

-- Update statistics for better query optimization
UPDATE STATISTICS Features;
UPDATE STATISTICS Layers;

-- Display index information
SELECT
    i.name AS IndexName,
    OBJECT_NAME(i.object_id) AS TableName,
    i.type_desc AS IndexType,
    i.is_unique AS IsUnique,
    i.is_primary_key AS IsPrimaryKey
FROM sys.indexes i
WHERE i.object_id IN (OBJECT_ID('Features'), OBJECT_ID('Layers'))
    AND i.name IS NOT NULL
ORDER BY TableName, IndexName;

-- Display table statistics
SELECT
    OBJECT_NAME(object_id) AS TableName,
    SUM(row_count) AS TotalRows
FROM sys.dm_db_partition_stats
WHERE object_id IN (OBJECT_ID('Features'), OBJECT_ID('Layers'))
    AND index_id < 2
GROUP BY object_id;

PRINT 'Index optimization completed successfully';