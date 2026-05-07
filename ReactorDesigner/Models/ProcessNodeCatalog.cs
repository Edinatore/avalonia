namespace ReactorDesigner.Models;

public static class ProcessNodeCatalog
{
    // Placeholder vector icons were sourced from Iconify-hosted open icon sets and embedded locally for stable runtime loading.
    private static readonly IReadOnlyList<NodeTemplate> Templates =
    [
        new(
            NodeKind.Reactor,
            "Process Equipment",
            "Reactor",
            "Reaction vessel",
            "#8E3343",
            "M256 41.875c-60.562 0-60.547 15.14-75.688 60.563l-30.28 90.843l41.624 41.626c16.44-16.44 39.26-26.47 64.344-26.47c25.085 0 47.904 10.03 64.344 26.47l41.625-41.625l-30.283-90.843c-15.14-45.42-15.125-60.562-75.687-60.562zm0 196.844c-33.447 0-60.563 27.083-60.563 60.53s27.116 60.563 60.563 60.563s60.563-27.116 60.563-60.563s-27.116-60.53-60.563-60.53m-144.78 21.75l-63.532 71.655C15.92 367.947 2.813 375.52 33.093 427.97c30.28 52.447 43.406 44.88 90.312 35.28l93.813-19.22l15.218-56.874c-22.457-6.017-42.552-20.744-55.094-42.47c-12.544-21.723-15.267-46.51-9.25-68.967l-56.875-15.25zm289.56 0l-56.874 15.25c6.017 22.455 3.293 47.243-9.25 68.967c-12.542 21.725-32.637 36.452-55.094 42.47l15.22 56.874l93.812 19.22c46.906 9.6 60.03 17.167 90.312-35.28c30.28-52.45 17.173-60.023-14.594-95.845l-63.53-71.656z",
            [
                new PortTemplate("Feed A", PortDirection.Input, "ProcessStream"),
                new PortTemplate("Feed B", PortDirection.Input, "ProcessStream")
            ],
            [
                new PortTemplate("Product", PortDirection.Output, "ProcessStream"),
                new PortTemplate("Temperature", PortDirection.Output, "Instrumentation")
            ]),
        new(
            NodeKind.Pump,
            "Process Equipment",
            "Pump",
            "Flow booster",
            "#2A6EE8",
            "M2 21v-6h1.5a9.3 9.3 0 0 1-.5-3a9 9 0 0 1 9-9h10v6h-1.5c.32.94.5 1.95.5 3a9 9 0 0 1-9 9zm3-9c0 1.28.34 2.47.94 3.5l3.46-2c-.25-.44-.4-.95-.4-1.5c0-.65.21-1.25.56-1.74L6.3 7.93C5.5 9.08 5 10.5 5 12m7 7c2.59 0 4.85-1.41 6.06-3.5l-3.46-2c-.52.9-1.49 1.5-2.6 1.5h-.29l-.38 3.97zm0-10c1.21 0 2.26.72 2.73 1.76l3.64-1.66A6.99 6.99 0 0 0 12 5zm0 2c-.55 0-1 .45-1 1s.45 1 1 1s1-.45 1-1s-.45-1-1-1",
            [new PortTemplate("Suction", PortDirection.Input, "ProcessStream")],
            [new PortTemplate("Discharge", PortDirection.Output, "ProcessStream")]),
        new(
            NodeKind.HeatExchanger,
            "Process Equipment",
            "Heat Exchanger",
            "Thermal transfer",
            "#D47D1E",
            "M7.95 3L6.53 5.19L7.95 7.4h-.01l-1.99 3.1l-1.73-.9l1.42-2.21l-1.42-2.2l2-3.1zm6-.11L12.53 5.1l1.42 2.2l-.01.01l-1.99 3.09l-1.73-.9l1.42-2.2l-1.42-2.2l2-3.1zm6.05 0L18.56 5.1L20 7.3v.01l-2 3.09l-1.75-.9l1.42-2.2l-1.42-2.2l2-3.1zM2 22v-8a2 2 0 0 1 2-2h16a2 2 0 0 1 2 2v8h-2v-2H4v2zm4-8a1 1 0 0 0-1 1v2a1 1 0 0 0 1 1a1 1 0 0 0 1-1v-2a1 1 0 0 0-1-1m4 0a1 1 0 0 0-1 1v2a1 1 0 0 0 1 1a1 1 0 0 0 1-1v-2a1 1 0 0 0-1-1m4 0a1 1 0 0 0-1 1v2a1 1 0 0 0 1 1a1 1 0 0 0 1-1v-2a1 1 0 0 0-1-1m4 0a1 1 0 0 0-1 1v2a1 1 0 0 0 1 1a1 1 0 0 0 1-1v-2a1 1 0 0 0-1-1",
            [
                new PortTemplate("Hot In", PortDirection.Input, "ProcessStream"),
                new PortTemplate("Cold In", PortDirection.Input, "ProcessStream")
            ],
            [
                new PortTemplate("Hot Out", PortDirection.Output, "ProcessStream"),
                new PortTemplate("Cold Out", PortDirection.Output, "ProcessStream")
            ]),
        new(
            NodeKind.Valve,
            "Control & Flow",
            "Valve",
            "Flow regulation",
            "#D7BD31",
            "M11 8V5H7V3h10v2h-4v3zM4 21v-8h2v1h3v-3H8V9h8v2h-1v3h3v-1h2v8h-2v-1H6v1zm2-3h12v-2h-5v-5h-2v5H6zm6 0",
            [new PortTemplate("Inlet", PortDirection.Input, "ProcessStream")],
            [new PortTemplate("Outlet", PortDirection.Output, "ProcessStream")]),
        new(
            NodeKind.PipeJunction,
            "Control & Flow",
            "Pipe Junction",
            "Flow splitting tee",
            "#2BA8BF",
            "M25 115v154h30V115zm432 0v154h30V115zM73 128v128h103.8l40-53.4l14.4 10.8l-39.2 52.3V439h128V265.7l-39.2-52.3l14.4-10.8l40 53.4H439V128zm23 23h320v18H96zm119 137h18v128h-18zm-36 169v30h154v-30z",
            [new PortTemplate("Main Feed", PortDirection.Input, "ProcessStream")],
            [
                new PortTemplate("Branch A", PortDirection.Output, "ProcessStream"),
                new PortTemplate("Branch B", PortDirection.Output, "ProcessStream")
            ]),
        new(
            NodeKind.Turbine,
            "Power Conversion",
            "Turbine",
            "Mechanical power",
            "#6952D5",
            "M2 12c0 5.5 4.5 10 10 10s10-4.5 10-10S17.5 2 12 2S2 6.5 2 12m18 0c0 4.4-3.6 8-8 8s-8-3.6-8-8s3.6-8 8-8s8 3.6 8 8m-7.5-5l-.3 1.3l-.9-3.3c-1 .3-1.6 1.3-1.4 2.4l.3 1.3l-2.3-2.4c-.7.7-.7 2 0 2.7l1 1l-3.3-.9c-.3 1 .3 2.1 1.4 2.4l1.3.3l-3.3.9c.3 1 1.3 1.6 2.4 1.4l1.3-.3l-2.4 2.4c.8.7 2 .7 2.7 0l.9-.9l-.9 3.3c1 .3 2.1-.3 2.4-1.4l.3-1.3l.9 3.3c1-.3 1.6-1.3 1.4-2.4l-.3-1.3l2.4 2.4c.7-.8.7-2 0-2.7l-1-1.2l3.3.9c.3-1-.3-2.1-1.4-2.4l-1.3-.3l3.3-.9c-.3-1-1.3-1.6-2.4-1.4l-1.3.3l2.4-2.4c-.8-.7-2-.7-2.7 0l-.9 1l.9-3.3c-1.1-.2-2.2.4-2.5 1.5m1 5c0 .8-.7 1.5-1.5 1.5s-1.5-.7-1.5-1.5s.7-1.5 1.5-1.5s1.5.7 1.5 1.5",
            [new PortTemplate("Steam In", PortDirection.Input, "ProcessStream")],
            [
                new PortTemplate("Exhaust", PortDirection.Output, "ProcessStream"),
                new PortTemplate("Power", PortDirection.Output, "Energy")
            ]),
        new(
            NodeKind.Sensor,
            "Instrumentation",
            "Sensor",
            "Measurement point",
            "#2EAC61",
            "M6 8v2h12V8h-3V2h2v4h5v2h-2v12a1 1 0 0 1-1 1H5a1 1 0 0 1-1-1V8H2V6h5V2h2v6zm7-6v6h-2V2z",
            [new PortTemplate("Process Tap", PortDirection.Input, "ProcessStream")],
            [new PortTemplate("Signal", PortDirection.Output, "Instrumentation")])
    ];

    public static IReadOnlyList<NodeTemplate> GetTemplates()
    {
        return Templates;
    }

    public static NodeTemplate GetTemplate(NodeKind kind)
    {
        return Templates.First(template => template.Kind == kind);
    }
}
