{
  description = "A Nix-flake-based C# development environment";

  inputs.nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";

  outputs = { self, nixpkgs }:
    let
      supportedSystems = [ "x86_64-linux" "aarch64-linux" "x86_64-darwin" "aarch64-darwin" ];
      forEachSupportedSystem = f: nixpkgs.lib.genAttrs supportedSystems (system: f {
        pkgs = import nixpkgs { inherit system; };
      });
    in
    {
      devShells = forEachSupportedSystem ({ pkgs }: {
        default = pkgs.mkShell {
          DOTNET_ROOT = "${pkgs.dotnet-sdk}/share/dotnet";
          packages = with pkgs; [
            dotnetCorePackages.sdk_9_0
            dotnetPackages.Nuget
            omnisharp-roslyn
            mono
          ];
        };
      });
    };
}
