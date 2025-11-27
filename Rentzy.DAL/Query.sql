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

INSERT INTO ApprovalStatus ( Name) VALUES
( 'Pending'),
( 'Approved'),
( 'Rejected'),
( 'Cancelled');

INSERT INTO BookingStatuses ( Name)
VALUES 
    ( 'Active'),
    ( 'Completed'),
    ( 'Cancelled');


    INSERT INTO PaymentMethod (Name)
VALUES 
('Credit Card'),
('Debit Card'),
('Bank Transfer'),
('Cash'),
('PayPal');


INSERT INTO PaymentStatus (Name)
VALUES
('Pending'),
('Paid'),
('Failed');
