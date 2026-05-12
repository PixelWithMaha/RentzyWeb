INSERT INTO Cities (Name) VALUES
('Karachi'),
('Lahore'),
('Islamabad'),
('Rawalpindi'),
('Faisalabad'),
('Multan'),
('Peshawar'),
('Quetta'),
('Sialkot'),
('Gujranwala'),
('Hyderabad'),
('Sukkur'),
('Bahawalpur'),
('Sargodha'),
('Rahim Yar Khan');

INSERT INTO PropertyTypes (Name) VALUES
('Apartment'),
('House'),
('Flat'),
('Room'),
('Studio'),
('Villa'),
('Commercial Shop'),
('Office'),
('Warehouse'),
('Farmhouse');

INSERT INTO ApprovalStatus (Name) VALUES
('Pending'),
('Approved'),
('Rejected'),
('Cancelled');

INSERT INTO BookingStatuses (Name)
VALUES 
('Active'),
('Completed'),
('Cancelled');

INSERT INTO Users (
    Phone,
    PasswordHash,
    Role,
    CreatedAt,
    Discriminator,
    Email,
    FirstName,
    IsVerified,
    LastName
)
VALUES
(
    '03001234567',
    '$2a$11$kcmtKfQ4cRW6OXa7dR1P0e1V.5kI6WoZuNpf4gO/iIfLSyJzCzWvC',
    'Landlord',
    GETUTCDATE(),
    'Landlord',
    'ali.khan@example.com',
    'Ali',
    1,
    'Khan'
),
(
    '03119876543',
    '$2a$11$kcmtKfQ4cRW6OXa7dR1P0e1V.5kI6WoZuNpf4gO/iIfLSyJzCzWvC',
    'Landlord',
    GETUTCDATE(),
    'Landlord',
    'sara.ahmed@example.com',
    'Sara',
    1,
    'Ahmed'
);



-- =========================================
--    ORIGIN/MASTER INSERTS
-- =========================================

INSERT INTO Properties (Title, Address, LandlordId, CityId, Description, MonthlyRent, PropertyTypeId)
VALUES
-- Landlord 1
('Luxury Apartment', 'Street 15, DHA Phase 6, Karachi', 3, 1, 'A luxury furnished apartment in DHA.', 55000, 1),
('Family House', 'Block H, Gulberg, Lahore', 4, 2, 'Spacious family home with 3 bedrooms.', 75000, 2),

-- Landlord 2
('Studio Flat', 'Sector F-7, Islamabad', 4, 3, 'Compact studio flat ideal for students.', 28000, 5),
('Commercial Shop', 'Saddar, Rawalpindi', 3, 4, 'Shop located in a busy commercial area.', 90000, 7);

INSERT INTO PropertyImages (ImageUrl, PropertyId)
VALUES
('https://picsum.photos/300/200?random=1', 1),
('https://picsum.photos/300/200?random=2', 1),
('https://picsum.photos/300/200?random=3', 2),
('https://picsum.photos/300/200?random=4', 2),
('https://picsum.photos/300/200?random=5', 3),
('https://picsum.photos/300/200?random=6', 4);

-- These are from origin/master (plural tables)
INSERT INTO PaymentMethods (Name) VALUES ('Credit Card');
INSERT INTO PaymentMethods (Name) VALUES ('Debit Card');
INSERT INTO PaymentMethods (Name) VALUES ('Cash');

INSERT INTO PaymentStatuses (Name) VALUES ('Paid');
INSERT INTO PaymentStatuses (Name) VALUES ('UnPaid');
INSERT INTO PaymentStatuses (Name) VALUES ('Failed');

INSERT INTO Payments 
    (Amount, Method, PaidAt, BookingId, StatusId, PaymentMethodId)
VALUES 
    (55000, 'Credit Card', '2025-11-28 00:50:28', 1, 1, 1);
INSERT INTO Users (
    Phone,
    PasswordHash,
    Role,
    CreatedAt,
    Discriminator,
    Email,
    FirstName,
    IsVerified,
    LastName
)
VALUES
(
    '03001234560',
    '$2a$11$kcmtKfQ4cRW6OXa7dR1P0e1V.5kI6WoZuNpf4gO/iIfLSyJzCzWvC',
    'Admin',
    GETUTCDATE(),
    'Admin',
    'ali.umer@example.com',
    'Ali',
    1,
    'Umer'
);
INSERT INTO UserStatuses ( UserId,IsActive,IsDeleted) VALUES (5,1,0);
INSERT INTO UserStatuses ( UserId,IsActive,IsDeleted) VALUES (3,1,0);