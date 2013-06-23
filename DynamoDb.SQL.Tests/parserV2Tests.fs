﻿// Copyright (c) Yan Cui 2012

// Email : theburningmonk@gmail.com
// Blog  : http://theburningmonk.com

namespace DynamoDb.SQL.Parser.Tests

open FsUnit
open NUnit.Framework
open DynamoDb.SQL
open DynamoDb.SQL.Parser

module V2Tests = 
    let equal = FsUnit.equal

    [<TestFixture>]
    type ``Given a V2 query`` () =
        [<Test>]
        [<ExpectedException(typeof<InvalidQuery>)>]
        member this.``when there is no attributes in the select clause it should except`` () =
            let select = "SELECT FROM Employees WHERE FirstName = \"Yan\""
            parseDynamoQueryV2 select |> should throw typeof<InvalidQuery>

        [<Test>]
        [<ExpectedException(typeof<InvalidTableName>)>]
        member this.``when there is no table name in the from clause it should except`` () =
            let select = "SELECT * FROM WHERE FirstName = \"Yan\""
            parseDynamoQueryV2 select |> should throw typeof<InvalidTableName>

        [<Test>]
        [<ExpectedException(typeof<InvalidQuery>)>]
        member this.``when there is no where clause it should except`` () =
            let select = "SELECT * FROM Employees"
            parseDynamoQueryV2 select |> should throw typeof<InvalidQuery>

        [<Test>]
        [<ExpectedException(typeof<InvalidQuery>)>]
        member this.``when there is no filter conditions in the where clause it should except`` () =
            let select = "SELECT * FROM Employees WHERE"
            parseDynamoQueryV2 select |> should throw typeof<InvalidQuery>

        [<Test>]
        [<ExpectedException(typeof<InvalidQuery>)>]
        member this.``when there is a HashKey keyword in the where clause it should except`` () =
            let select = "SELECT * FROM Employees WHERE @HashKey = \"Yan\""
            parseDynamoQueryV2 select |> should throw typeof<InvalidQuery>

        [<Test>]
        [<ExpectedException(typeof<InvalidQuery>)>]
        member this.``when there is a RangeKey keyword in the where clause it should except`` () =
            let select = "SELECT * FROM Employees WHERE @RangeKey = \"Yan\""
            parseDynamoQueryV2 select |> should throw typeof<InvalidQuery>

        [<Test>]
        member this.``when there is white spaces around the attribute names and table name they should be ignored`` () =
            let select = "SELECT Name,    Age,
                                 Salary
                          FROM   Employees 
                            WHERE FirstName   =      \"Yan\"
                          LIMIT     5"

            let query = parseDynamoQueryV2 select

            query.Action    |> should equal <| Select [ Attribute "Name"; Attribute "Age"; Attribute "Salary" ]
            query.From      |> should equal <| From "Employees"
            query.Where     |> should equal <| Where [ (Attribute "FirstName", Equal (S "Yan")) ]
            query.Limit     |> should equal <| Some(Limit 5)

        [<Test>]
        member this.``when the SELECT, FROM, WHERE and LIMIT keywords are not in capitals they should still be parsed correctly`` () =
            let select = "sELeCT Name, Age, Salary
                          FrOm Employees
                          where FirstName = \"Yan\"
                          liMIt 5"

            let query = parseDynamoQueryV2 select

            query.Action    |> should equal <| Select [ Attribute "Name"; Attribute "Age"; Attribute "Salary" ]
            query.From      |> should equal <| From "Employees"
            query.Where     |> should equal <| Where [ (Attribute "FirstName", Equal (S "Yan")) ]
            query.Limit     |> should equal <| Some(Limit 5)

        [<Test>]
        member this.``when there are multiple conditions in the where clause they should all be parsed`` () =
            let select = "SELECT * FROM Employees WHERE FirstName = \"Yan\" AND LastName = \"Cui\""

            let query = parseDynamoQueryV2 select

            query.Action    |> should equal <| Select [ Asterisk ]
            query.From      |> should equal <| From "Employees"
            query.Where     |> should equal <| Where [ (Attribute "FirstName", Equal(S "Yan")); (Attribute "LastName", Equal(S "Cui")) ]
            query.Limit     |> should equal <| None

        [<Test>]
        [<ExpectedException(typeof<InvalidQuery>)>]
        member this.``when the != operator is used it should except`` () =
            let select = "SELECT * FROM Employees WHERE FirstName = \"Yan\" AND Age != 30"
            parseDynamoQueryV2 select |> should throw typeof<InvalidQuery>

        [<Test>]
        member this.``when < operator is used it should be parsed correctly`` () =
            let select = "SELECT * FROM Employees WHERE FirstName = \"Yan\" AND Age < 99"

            let query = parseDynamoQueryV2 select

            query.Action    |> should equal <| Select [ Asterisk ]
            query.From      |> should equal <| From "Employees"
            query.Where     |> should equal <| Where [ (Attribute "FirstName", Equal (S "Yan")); (Attribute "Age", LessThan (N 99.0)) ]
            query.Limit     |> should equal <| None

        [<Test>]
        member this.``when <= operator is used it should be parsed correctly`` () =
            let select = "SELECT * FROM Employees WHERE FirstName = \"Yan\" AND Age <= 99"

            let query = parseDynamoQueryV2 select

            query.Action    |> should equal <| Select [ Asterisk ]
            query.From      |> should equal <| From "Employees"
            query.Where     |> should equal <| Where [ (Attribute "FirstName", Equal (S "Yan")); (Attribute "Age", LessThanOrEqual (N 99.0)) ]
            query.Limit     |> should equal <| None

        [<Test>]
        member this.``when > operator is used it should be parsed correctly`` () =
            let select = "SELECT * FROM Employees WHERE FirstName = \"Yan\" AND Age > 99"

            let query = parseDynamoQueryV2 select

            query.Action    |> should equal <| Select [ Asterisk ]
            query.From      |> should equal <| From "Employees"
            query.Where     |> should equal <| Where [ (Attribute "FirstName", Equal (S "Yan")); (Attribute "Age", GreaterThan (N 99.0)) ]
            query.Limit     |> should equal <| None

        [<Test>]
        member this.``when >= operator is used it should be parsed correctly`` () =
            let select = "SELECT * FROM Employees WHERE FirstName = \"Yan\" AND Age >= 99"

            let query = parseDynamoQueryV2 select

            query.Action    |> should equal <| Select [ Asterisk ]
            query.From      |> should equal <| From "Employees"
            query.Where     |> should equal <| Where [ (Attribute "FirstName", Equal (S "Yan")); (Attribute "Age", GreaterThanOrEqual (N 99.0)) ]
            query.Limit     |> should equal <| None

        [<Test>]
        [<ExpectedException(typeof<InvalidQuery>)>]
        member this.``when the Contains operator is used it should except`` () =
            let select = "SELECT * FROM Employees WHERE FirstName = \"Yan\" AND LastName CONTAINS \"Cui\""
            parseDynamoQueryV2 select |> should throw typeof<InvalidQuery>

        [<Test>]
        [<ExpectedException(typeof<InvalidQuery>)>]
        member this.``when the NotContains operator is used it except`` () =
            let select = "SELECT * FROM Employees WHERE FirstName = \"Yan\" AND LastName NOT CONTAINS \"Cui\""
            parseDynamoQueryV2 select |> should throw typeof<InvalidQuery>

        [<Test>]
        member this.``when the Begins With operator is used it should be parsed correctly`` () =
            let select = "SELECT * FROM Employees WHERE FirstName = \"Yan\" AND LastName BEGINS WITH \"Cui\""

            let query = parseDynamoQueryV2 select

            query.Action    |> should equal <| Select [ Asterisk ]
            query.From      |> should equal <| From "Employees"
            query.Where     |> should equal <| Where [ (Attribute "FirstName", Equal (S "Yan")); (Attribute "LastName", BeginsWith (S "Cui")) ]
            query.Limit     |> should equal <| None
    
        [<Test>]
        member this.``when the Between operator is used it should be parsed correctly`` () =
            let select = "SELECT * FROM Employees WHERE FirstName = \"Yan\" AND Age BETWEEN 10 AND 30"

            let query = parseDynamoQueryV2 select

            query.Action    |> should equal <| Select [ Asterisk ]
            query.From      |> should equal <| From "Employees"
            query.Where     |> should equal <| Where [ (Attribute "FirstName", Equal (S "Yan")); (Attribute "Age", Between ((N 10.0), (N 30.0))) ]
            query.Limit     |> should equal <| None

        [<Test>]
        [<ExpectedException(typeof<InvalidQuery>)>]
        member this.``when the In operator is used it should except`` () =
            let select = "SELECT * FROM Employees WHERE FirstName = \"Yan\" AND Age IN (\"Foo\", \"Bar\")"
            parseDynamoQueryV2 select |> should throw typeof<InvalidQuery>

        [<Test>]
        [<ExpectedException(typeof<InvalidQuery>)>]
        member this.``when the Is Null operator is used it should except`` () =
            let select = "SELECT * FROM Employees WHERE FirstName = \"Yan\" AND LastName IS NULL"
            parseDynamoQueryV2 select |> should throw typeof<InvalidQuery>

        [<Test>]
        [<ExpectedException(typeof<InvalidQuery>)>]
        member this.``when the Is Not Null operator is used it should except`` () =
            let select = "SELECT * FROM Employees WHERE FirstName = \"Yan\" AND LastName IS NOT NULL"
            parseDynamoQueryV2 select |> should throw typeof<InvalidQuery>

        [<Test>]
        member this.``when limit clause is specified, it should be parsed correctly`` () =
            let select = "SELECT * FROM Employees WHERE FirstName = \"Yan\" AND Age >= 30 LIMIT 10"

            let query = parseDynamoQueryV2 select
            
            query.Action    |> should equal <| Select [ Asterisk ]
            query.From      |> should equal <| From "Employees"
            query.Where     |> should equal <| Where [ (Attribute "FirstName", Equal (S "Yan")); (Attribute "Age", GreaterThanOrEqual(N 30.0)) ]
            query.Limit     |> should equal <| Some(Limit 10)

        [<Test>]
        member this.``when order asc is specified, it should be parsed correctly`` () =
            let select = "SELECT * FROM Employees WHERE FirstName = \"Yan\" AND Age >= 30 ORDER ASC LIMIT 10"

            let query = parseDynamoQueryV2 select

            query.Action    |> should equal <| Select [ Asterisk ]
            query.From      |> should equal <| From "Employees"
            query.Where     |> should equal <| Where [ (Attribute "FirstName", Equal (S "Yan")); (Attribute "Age", GreaterThanOrEqual(N 30.0)) ]
            query.Limit     |> should equal <| Some(Limit 10)
            query.Order     |> should equal <| Some(Asc)

        [<Test>]
        member this.``when order desc is specified, it should be parsed correctly`` () =
            let select = "SELECT * FROM Employees WHERE FirstName = \"Yan\" AND Age >= 30 ORDER DESC LIMIT 10"

            let query = parseDynamoQueryV2 select

            query.Action    |> should equal <| Select [ Asterisk ]
            query.From      |> should equal <| From "Employees"
            query.Where     |> should equal <| Where [ (Attribute "FirstName", Equal (S "Yan")); (Attribute "Age", GreaterThanOrEqual(N 30.0)) ]
            query.Limit     |> should equal <| Some(Limit 10)
            query.Order     |> should equal <| Some(Desc)

        [<Test>]
        member this.``when a count query is specified, it should be parsed correctly`` () =
            let count = "COUNT * FROM Employees WHERE FirstName = \"Yan\" AND Age >= 30 ORDER DESC LIMIT 10"

            let query = parseDynamoQueryV2 count

            query.Action    |> should equal <| Count
            query.From      |> should equal <| From "Employees"
            query.Where     |> should equal <| Where [ (Attribute "FirstName", Equal (S "Yan")); (Attribute "Age", GreaterThanOrEqual(N 30.0)) ]
            query.Limit     |> should equal <| Some(Limit 10)
            query.Order     |> should equal <| Some(Desc)

        [<Test>]
        [<ExpectedException(typeof<InvalidQuery>)>]
        member this.``when a count query is specified with attribute names, it should except`` () =
            let count = "COUNT FirstName FROM Employees WHERE FirstName = \"Yan\" AND Age >= 30 ORDER DESC LIMIT 10"
            parseDynamoQueryV2 count |> should throw typeof<InvalidQuery>

        [<Test>]
        member this.``when NoConsistentRead option is specified it should be captured in the Options clause`` () =
            let select = "SELECT * FROM Employees WHERE FirstName = \"Yan\" WITH (  nOConsiStentRead )"
        
            let query = parseDynamoQueryV2 select

            query.Action    |> should equal <| Select [ Asterisk ]
            query.From      |> should equal <| From "Employees"
            query.Where     |> should equal <| Where [ (Attribute "FirstName", Equal (S "Yan")) ]
            query.Options   |> should equal <| Some [| NoConsistentRead |]

        [<Test>]
        member this.``when PageSize option is specified it should be captured in the Options clause`` () =
            let select = "SELECT * FROM Employees WHERE FirstName = \"Yan\" WITH (Pagesize(  10) )"
        
            let query = parseDynamoQueryV2 select

            query.Action    |> should equal <| Select [ Asterisk ]
            query.From      |> should equal <| From "Employees"
            query.Where     |> should equal <| Where [ (Attribute "FirstName", Equal (S "Yan")) ]
            query.Options   |> should equal <| Some [| QueryPageSize 10 |]

        [<Test>]
        member this.``when Index option is specified with AllAttributes set to true it should be captured in the Options clause`` () =
            let select = "SELECT * FROM Employees WHERE FirstName = \"Yan\" WITH (Index( _M-y.1nd3x ,  true) )"
        
            let query = parseDynamoQueryV2 select

            query.Action    |> should equal <| Select [ Asterisk ]
            query.From      |> should equal <| From "Employees"
            query.Where     |> should equal <| Where [ (Attribute "FirstName", Equal (S "Yan")) ]
            query.Options   |> should equal <| Some [| Index("_M-y.1nd3x", true) |]

        [<Test>]
        member this.``when Index option is specified with AllAttributes set to false it should be captured in the Options clause`` () =
            let select = "SELECT * FROM Employees WHERE FirstName = \"Yan\" WITH (Index( _M-y.1nd3x ,  false) )"
        
            let query = parseDynamoQueryV2 select

            query.Action    |> should equal <| Select [ Asterisk ]
            query.From      |> should equal <| From "Employees"
            query.Where     |> should equal <| Where [ (Attribute "FirstName", Equal (S "Yan")) ]
            query.Options   |> should equal <| Some [| Index("_M-y.1nd3x", false) |]

        [<Test>]
        member this.``when NoReturnedCapacity option is specified it should be captured in the Options clause`` () =
            let select = "SELECT * FROM Employees WHERE FirstName = \"Yan\" WITH ( NoReturnedCapacity)"
        
            let query = parseDynamoQueryV2 select

            query.Action    |> should equal <| Select [ Asterisk ]
            query.From      |> should equal <| From "Employees"
            query.Where     |> should equal <| Where [ (Attribute "FirstName", Equal (S "Yan")) ]
            query.Options   |> should equal <| Some [| QueryNoReturnedCapacity |]

        [<Test>]
        member this.``when both NoConsistentRead and PageSize options are specified they should be captured in the Options clause`` () =
            let select = "SELECT * FROM Employees WHERE FirstName = \"Yan\" WITH ( NOconsistentRead, Pagesize(  10) )"
        
            let query = parseDynamoQueryV2 select

            query.Action    |> should equal <| Select [ Asterisk ]
            query.From      |> should equal <| From "Employees"
            query.Where     |> should equal <| Where [ (Attribute "FirstName", Equal (S "Yan")) ]
            query.Options   |> should equal <| Some [| NoConsistentRead; QueryPageSize 10 |]
            
    /// NOTE: there's very little difference between V1 and V2 scan, only an additional scan option, so don't repeat
    /// existing V1 tests, only test the new option ScanNoReturnedCapacity
    [<TestFixture>]
    type ``Given a V2 scan`` () =
        [<Test>]
        member this.``when NoReturnedCapacity option is specified it should be captured in the Options clause`` () =
            let select = "SELECT * FROM Employees WITH ( NoReturnedCapacity )"
        
            let scan = parseDynamoScanV2 select

            scan.Action     |> should equal <| Select [ Asterisk ]
            scan.From       |> should equal <| From "Employees"
            scan.Where      |> should equal None
            scan.Limit      |> should equal None
            scan.Options    |> should equal <| Some [| ScanNoReturnedCapacity |]

        [<Test>]
        member this.``when ScanSegments option is specified it should be parsed correctly`` () =
            let select = "SELECT * FROM Employees WITH (Segments( 15 ) )"
        
            let scan = parseDynamoScanV2 select

            scan.Action     |> should equal <| Select [ Asterisk ]
            scan.From       |> should equal <| From "Employees"
            scan.Where      |> should equal <| None
            scan.Limit      |> should equal <| None
            scan.Options    |> should equal <| Some [| ScanSegments 15 |]