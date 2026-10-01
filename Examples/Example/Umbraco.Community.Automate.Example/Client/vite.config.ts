import { defineConfig } from "vite";
import { resolve } from "path";

// Builds src/ into ../wwwroot, which the package serves at /App_Plugins/UmbracoCommunityAutomateExample/.
// public/umbraco-package.json is copied over as-is and tells Umbraco to load the built bundle.
export default defineConfig({
    build: {
        lib: {
            entry: {
                "umbraco-community-automate-example-manifests": resolve(__dirname, "src/manifests.ts"),
            },
            formats: ["es"],
        },
        outDir: "../wwwroot",
        emptyOutDir: true,
        sourcemap: true,
        rollupOptions: {
            // Umbraco provides these at runtime; never bundle them.
            external: [/^@umbraco/],
        },
    },
});
