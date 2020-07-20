
build:
	@dotnet build KiteConnect/KiteConnect.csproj

pack:
	@dotnet pack -c Release KiteConnect/KiteConnect.csproj

clean:
	@dotnet clean

test:
	@dotnet test