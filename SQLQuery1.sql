select * from Rooms
select * from Tenants
ALTER TABLE Bills ADD PaidAt DATETIME2 NULL;
INSERT INTO Rooms (RoomNumber, Price, MaxOccupancy, Description, Status, CreatedAt)
VALUES ('A103', 2800000, 2, 'Phòng test SQL', 0, GETDATE())
,('B101', 3200000, 3, N'Phòng tầng 2 có ban công', 0, GETDATE()),
('B102', 3000000, 2, N'Phòng view đẹp', 0, GETDATE()),
('C101', 3500000, 4, N'Phòng rộng cho nhóm', 0, GETDATE()),
('C102', 2600000, 2, N'Phòng nhỏ giá rẻ', 0, GETDATE());

INSERT INTO Tenants (Name, Phone, CCCD, CreatedAt)
VALUES ('Le Van C', '0900000003', '003456789012', GETDATE());

UPDATE Bills
SET PaidAt = NULL
WHERE Status = 0

ALTER TABLE Bills
ALTER COLUMN PaidAt datetime NULL

select * from Users
select * from Contracts