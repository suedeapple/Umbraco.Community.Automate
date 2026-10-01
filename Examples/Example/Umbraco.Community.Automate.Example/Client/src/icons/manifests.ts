const icons: UmbExtensionManifest = {
    type: "icons",
    alias: "UmbracoCommunityAutomateExample.Icons",
    name: "Example Icons",
    js: () => import("./icons.js"),
};

export const iconManifests: UmbExtensionManifest[] = [icons];
