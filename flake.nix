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

        commonPublishFlags = [
          "-p:PublishSingleFile=true"
          "-p:PublishTrimmed=true"
          "-p:TrimMode=partial"
          "-p:IncludeNativeLibrariesForSelfExtract=true"
          "-p:EnableCompressionInSingleFile=true"
        ];

        commonBuildConfig = {
          version = "0.1.0";
          src = ./.;
          dotnetTools = [ pkgs.dotnet-ef ];
          dotnet-sdk = pkgs.dotnetCorePackages.sdk_8_0;
          dotnet-runtime = pkgs.dotnetCorePackages.aspnetcore_8_0;
          nugetDeps = ./deps.json;
          buildType = "Release";
          solutionFile = "Kawsay.sln";
          projectFile = "src/Kawsay.Api/Api.csproj";
          enableParallelBuilding = true;
        };

        mkKawsayPackage = { environment }:
          pkgs.buildDotnetModule (commonBuildConfig // {
            pname = "kawsay-backend-${environment}";
            dotnetPublishFlags = commonPublishFlags ++ [
              "--self-contained"
              "--runtime" "linux-x64"
              "-p:EnvironmentName=${environment}"
            ];

            makeWrapperArgs = [
              "--set" "ASPNETCORE_ENVIRONMENT" "${environment}"
              "--add-flags" "--contentRoot"
              "--add-flags" "."
            ];

            meta = with pkgs.lib; {
              description = "Kawsay backend API (${environment})";
              license = licenses.mit;
              platforms = platforms.linux;
              maintainers = [ "zrock" ];
            };
          });

      in {
        packages = {
          development = mkKawsayPackage { environment = "Development"; };
          staging = mkKawsayPackage { environment = "Staging"; };
          production = mkKawsayPackage { environment = "Production"; };
          default = self.packages.${system}.development;

          dockerImage = pkgs.dockerTools.buildImage {
            name = "kawsay-backend";
            tag = "latest";
            copyToRoot = self.packages.${system}.production;
            config = {
              Env = [ "ASPNETCORE_URLS=http://+:5167" ];
              Cmd = [ "/bin/Api" ];
              ExposedPorts = { "5167/tcp" = {}; };
            };
          };

          generateDeps = pkgs.writeShellApplication {
            name = "generate-nuget-deps";
            runtimeInputs = with pkgs; [
              dotnetCorePackages.sdk_8_0
              nuget-to-json
              jq
            ];
            text = ''
              echo "🧹 Cleaning up previous runs..."
              rm -rf out/ || true

              echo "📦 Restoring NuGet packages..."
              dotnet restore --packages out --verbosity minimal

              echo "🔧 Generating deps.json..."
              nuget-to-json out > deps.json

              echo "✅ Generated deps.json successfully"
              echo "📊 Package count: $(jq 'length' deps.json)"
            '';
          };

          migrate = pkgs.writeShellApplication {
            name = "kawsay-migrate";
            runtimeInputs = [
              pkgs.dotnet-ef
              pkgs.dotnetCorePackages.sdk_8_0
            ];
            text = ''
              set -e
              if [ -z "$KAWSAY_CONNECTION_STRING" ]; then
                echo "Error: KAWSAY_CONNECTION_STRING environment variable is not set."
                exit 1
              fi

              echo "Executing EF Core Migrations..."
              dotnet ef database update \
                --project src/Kawsay.Infrastructure/Infrastructure.csproj \
                --startup-project src/Kawsay.Api/Api.csproj \
                -- --connection "$KAWSAY_CONNECTION_STRING"

              echo "✅ Migrations applied successfully."
            '';
          };
        };

        devShells.default = pkgs.mkShell {
          buildInputs = with pkgs; [
            dotnetCorePackages.sdk_8_0
            dotnet-ef
          ];

          shellHook = ''
              set -e
              if [ -z "$KAWSAY_CONNECTION_STRING" ]; then
                echo "Error: KAWSAY_CONNECTION_STRING environment variable is not set."
                exit 1
              fi
          '';

          DOTNET_CLI_TELEMETRY_OPTOUT = "1";
          DOTNET_NOLOGO = "1";
          ASPNETCORE_ENVIRONMENT = "Development";
        };

        apps = {
          default = flake-utils.lib.mkApp {
            drv = self.packages.${system}.development;
            exePath = "/bin/Api";
          };

          generateDeps = flake-utils.lib.mkApp {
            drv = self.packages.${system}.generateDeps;
          };

          migrate = flake-utils.lib.mkApp {
            drv = self.packages.${system}.migrate;
          };
        };

        formatter = pkgs.nixpkgs-fmt;
      }
    );
}
