#potentially need to add native google protos too, ie C:\ProgramData\chocolatey\lib\protoc\tools\include on my machine
#
protoc.exe --proto_path=protos --csharp_out=gen --csharp_opt=base_namespace=InventoryProtos protos/*.proto
