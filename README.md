# CollabClient

## Language: C# 7.0 
## Framework: .NET 

## Project
wtp_collab environment, which aims to provide friendly tool for collaborators to use our data 

## Description:
The collabclient is a software with GUI that people can combine different columns from different tables to be a giant data files. Usually, it is easy to do it in Access. However, since I decided to use SQLite instead of Mysql database to store tables for collaborators, Access is no longer a viable solution, and we need to build our own GUI for others to make data files. I use Visual Studio 2017 (essential to use it) to develop the GUI, and C# 7.0 to code the backend. The collab client is asked to help collaborator choose their tables and columns fast, and easy to add special flags. It also helps collaborators to output the datafile in the form of csv, which can be recognzied by many data analyze tool, including access itself. One challenge for this collab client is that every collaborator is to keep track of who has made a datafile and when. This log information currently will enter the wtp_collab database, and then gets copied back to the main database.

## Component:
*   CollabClient.sln: load this to Visual Studio, and it will lay out the whole project for you. Other technique details will be explained in the code or another specification file. 

## High-Level Abstraction:
* Data Source: wtp_collab Sqlite db placed in the wtp_collab drive
* Output Source: wtp_collab Sqlite db, and csv file to folder *user_make_tables*

## How to deploy
* If there is no additional library you need to add, it's OK for you to build, and then copy the "CollabClient.exe" under bin/Debug to the **O:\CollabClient\Debug** (O is the drive that we conventionally assign to the wtp_collab. It might not work if wtp_collab is not mapped to O). If you have more library, it should be OK to copy the whole Debug to overwrite the original content. 

## Future
* Improve the stability of the program
* Further test and polish the algorithm of inner joining
* More user friendly

>"Updated by JJ on 06/09/2017"
