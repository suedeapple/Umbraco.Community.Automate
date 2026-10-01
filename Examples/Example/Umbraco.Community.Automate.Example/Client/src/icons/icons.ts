// The icon name matches Icon = "icon-automate-example" on the C# attributes (icon-automate-<area>).
export default [
    {
        name: "icon-automate-example",
        path: () => import("./example.icon.js"),
        keywords: ["example", "httpbin", "automate"],
    },
];
