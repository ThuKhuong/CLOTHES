-- Backfill CTHD.GIAMGIA based on HOADON.GIAMGIA for existing invoices
-- This uses a simple allocation strategy:
--   - Allocate discount proportionally by line amount (SL * DONGIA)
--   - Put rounding remainder into the last line of each invoice
-- Notes:
--   - Only updates invoices where HOADON.GIAMGIA > 0 and all CTHD.GIAMGIA are 0

SET NOCOUNT ON;

;WITH cte AS (
    SELECT
        ct.SOHD,
        ct.MACT,
        ct.SL,
        ct.DONGIA,
        CAST(ct.SL * ct.DONGIA AS decimal(18,2)) AS LINE_TOTAL,
        CAST(hd.GIAMGIA AS decimal(18,2)) AS HD_GIAM,
        CAST(hd.TONGTIEN1 AS decimal(18,2)) AS HD_TONG1,
        ROW_NUMBER() OVER (PARTITION BY ct.SOHD ORDER BY ct.MACT) AS RN,
        COUNT(*) OVER (PARTITION BY ct.SOHD) AS CNT
    FROM dbo.CTHD ct
    JOIN dbo.HOADON hd ON hd.SOHD = ct.SOHD
    WHERE
        ISNULL(hd.GIAMGIA, 0) > 0
        AND EXISTS (
            SELECT 1
            FROM dbo.CTHD ct2
            WHERE ct2.SOHD = ct.SOHD
            GROUP BY ct2.SOHD
            HAVING SUM(ISNULL(ct2.GIAMGIA, 0)) = 0
        )
), alloc AS (
    SELECT
        SOHD,
        MACT,
        RN,
        CNT,
        LINE_TOTAL,
        HD_GIAM,
        HD_TONG1,
        CAST(ROUND(CASE WHEN HD_TONG1 = 0 THEN 0 ELSE (HD_GIAM * LINE_TOTAL / HD_TONG1) END, 0) AS decimal(18,2)) AS ALLOC0
    FROM cte
), sums AS (
    SELECT
        SOHD,
        SUM(CASE WHEN RN < CNT THEN ALLOC0 ELSE 0 END) AS SUM_BEFORE_LAST
    FROM alloc
    GROUP BY SOHD
), final_alloc AS (
    SELECT
        a.SOHD,
        a.MACT,
        CAST(
            CASE
                WHEN a.HD_GIAM <= 0 OR a.HD_TONG1 <= 0 THEN 0
                WHEN a.RN = a.CNT THEN (a.HD_GIAM - s.SUM_BEFORE_LAST)
                ELSE a.ALLOC0
            END
        AS decimal(18,2)) AS ALLOC_GIAM
    FROM alloc a
    JOIN sums s ON s.SOHD = a.SOHD
)
UPDATE ct
SET ct.GIAMGIA = fa.ALLOC_GIAM
FROM dbo.CTHD ct
JOIN final_alloc fa
    ON fa.SOHD = ct.SOHD
    AND fa.MACT = ct.MACT;

SELECT 'UPDATED_ROWS' AS Info, @@ROWCOUNT AS AffectedRows;
