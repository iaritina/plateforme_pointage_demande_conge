CREATE INDEX IX_DemandeConges_UserId_DateDebut
    ON DemandeConges (UserId, DateDebut DESC);

CREATE INDEX IX_DemandeConges_Status_DateDebut
    ON DemandeConges (Status, DateDebut DESC);

CREATE INDEX IX_DemandeConges_DateDebut
    ON DemandeConges (DateDebut DESC);


CREATE INDEX IX_Users_Id ON [User] (Id);

CREATE INDEX IX_Users_LastName_FirstName
    ON [User] (LastName, FirstName);