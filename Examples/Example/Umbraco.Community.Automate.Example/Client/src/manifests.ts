import { iconManifests } from "./icons/manifests.js";
import { messageEditorManifests } from "./message-editor/manifests.js";

// Everything the package adds to the backoffice. Loaded by the bundle in public/umbraco-package.json.
export const manifests: Array<UmbExtensionManifest> = [...iconManifests, ...messageEditorManifests];
