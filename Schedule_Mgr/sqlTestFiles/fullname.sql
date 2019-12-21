SELECT Suffix, Firstname, Middlename, Lastname, 
coalesce(Firstname, '')
       || CASE
            WHEN coalesce(Firstname, '') <> ''
                 AND (coalesce(Middlename, '') <> '' <> '')
              THEN ' '
            ELSE
              ''
          END
       || coalesce(Middlename, '')
       || CASE
            WHEN coalesce(Middlename, '') <> ''
              THEN ' '
            ELSE
              ''
          END
       || coalesce(Lastname, '') AS Full_Name
FROM Accounts
