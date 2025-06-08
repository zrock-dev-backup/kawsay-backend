{
  description = "Kawsay backend build script";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs = { self, nixpkgs, flake-utils }:
    flake-utils.lib.eachDefaultSystem (system:
      let
        pkgs = nixpkgs.legacyPackages.${system};

        mkKawsayPackage = { environment }:
          pkgs.buildDotnetModule {
            pname = "kawsay-backend-${environment}";
            version = "0.1.0";
            src = ./.;
			dotnetTools = [ pkgs.dotnet-ef ];
            dotnet-sdk = pkgs.dotnetCorePackages.sdk_8_0;
			dotnet-runtime = pkgs.dotnetCorePackages.aspnetcore_8_0;
            nugetDeps = ./deps.json;
			buildType = "Release";
            solutionFile = "Kawsay.sln";
            projectFile = "src/Kawsay.Api/Api.csproj";
            executables = [ "Api" ];
			makeWrapperArgs = [ "--set" "ASPNETCORE_ENVIRONMENT" "${environment}" ];
          };

      in {
        packages = {
          development = mkKawsayPackage {
            environment = "Development";
          };
          staging = mkKawsayPackage {
            environment = "Staging";
          };
          production = mkKawsayPackage {
            environment = "Production";
          };
          default = self.packages.${system}.development;

          generateDeps = pkgs.writeShellApplication {
            name = "generate-nuget-deps";
            runtimeInputs = with pkgs; [
              dotnetCorePackages.sdk_8_0
              nuget-to-json
            ];
            text = ''
              echo "clean up"
			  rm -fr out/
              echo "Restoring NuGet packages..."
              dotnet restore --packages out
              echo "Generating deps.json..."
              nuget-to-json out > deps.json
              echo "Generated deps.json successfully"
            '';
          };
        };

        devShells.default = pkgs.mkShell {
          buildInputs = with pkgs; [
            dotnetCorePackages.sdk_8_0
            nuget-to-json
			pkgs.dotnet-ef
          ];

          shellHook = ''
            echo "Kawsay development environment"
            echo "Available commands:"
            echo "  - dotnet: .NET CLI"
            echo "  - nix run .#generateDeps: Generate NuGet dependencies"
            echo ""
            echo "To build: nix build .#development"
            echo "To run: nix run .#development"
          '';
        };
      }
    );
}
