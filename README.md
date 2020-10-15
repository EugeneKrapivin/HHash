![.NET Core](https://github.com/EugeneKrapivin/HHash/workflows/.NET%20Core/badge.svg?branch=master)

# HHash
Library for creating heirarchial identifiers.

## Description

Given a resource path `this/is/a/path/to/the/resource/<resourceId>` we can generate the ID in such a way that it is easy to verify if a given ID is part of the path.

## Motivation

Sometimes in restful APIs its a bit hard to know if a resource on the url actually belongs to the path of the url.
Moreover, many implementation will just use the resource ID in the path as the database primary key to access the data.

If a user have access to a different path, he could potentially traverse all the IDs is a proper validation is not conducted.
Inorder to properly validate a path, multiple requests are required to ensure all path elements are really descendents of one another.
This solution bleeds requests in the the datacenter. Meaning, an attacker could potentially just bleed your compute resources on dumb validations.

## Usage

The usage is pretty simple, and I suggest you take a look at the `PropertyTests` suite.

```csharp
var keymaker = new Hasher();
var key = keymaker.CreateId("hello", "world"); // should return a base64 encoded string

Console.WriteLine($"this key should be valid for path `[\"hello\", \"world\"]` > {sut.ValidateId(id, "hello", "world")}");

Console.WriteLine($"this key should be invalid for path `[\"hello\"]` > {sut.ValidateId(id, "hello", "world")}");
Console.WriteLine($"this key should be invalid for path `[\"world\", \"hello\"]` > {sut.ValidateId(id, "hello", "world")}");


