-- =============================================
-- Parte B — Consultas
-- =============================================

-- 1. Listado paginado de tickets por Status y Priority
DECLARE @Status NVARCHAR(20) = 'Open';     -- filtro, NULL = todos
DECLARE @Priority NVARCHAR(20) = NULL;     -- filtro, NULL = todos
DECLARE @Page INT = 1;
DECLARE @PageSize INT = 10;

SELECT t.Id, t.Title, t.Priority, t.Status, t.CreatedAt,
       u.DisplayName AS CreatorName,
       COUNT(c.Id)   AS CommentCount
FROM Ticket t
JOIN [User] u ON u.Id = t.CreatedBy
LEFT JOIN Comment c ON c.TicketId = t.Id
WHERE (@Status IS NULL OR t.Status = @Status)
  AND (@Priority IS NULL OR t.Priority = @Priority)
GROUP BY t.Id, t.Title, t.Priority, t.Status, t.CreatedAt, u.DisplayName
ORDER BY t.CreatedAt DESC
OFFSET (@Page - 1) * @PageSize ROWS
FETCH NEXT @PageSize ROWS ONLY;

-- 2. Top 5 usuarios que más tickets crearon en el último mes
SELECT TOP 5
    u.Id, u.Email, u.DisplayName,
    COUNT(t.Id) AS TicketsCreated
FROM [User] u
JOIN Ticket t ON t.CreatedBy = u.Id
WHERE t.CreatedAt >= DATEADD(MONTH, -1, SYSUTCDATETIME())
GROUP BY u.Id, u.Email, u.DisplayName
ORDER BY TicketsCreated DESC;

-- 3. Buscar tickets donde q aparezca en Title o Description
DECLARE @SearchTerm NVARCHAR(100) = 'error';

SELECT Id, Title, Description, Priority, Status, CreatedAt
FROM Ticket
WHERE Title LIKE '%' + @SearchTerm + '%'
   OR Description LIKE '%' + @SearchTerm + '%';

-- 4. Tickets atrasados: creados hace más de X días y NO cerrados
DECLARE @DaysThreshold INT = 7;

SELECT Id, Title, Priority, Status, CreatedAt
FROM Ticket
WHERE CreatedAt < DATEADD(DAY, -@DaysThreshold, SYSUTCDATETIME())
  AND Status != 'Closed';
