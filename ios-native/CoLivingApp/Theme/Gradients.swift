import SwiftUI

/// Six-point asymmetric Linen mesh with whisper Clay top-right and a cool
/// wash bottom-mid. Phase shifts over ~125s — barely visible, but the canvas
/// breathes instead of sitting flat.
struct AppBackground: View {
    var body: some View {
        TimelineView(.animation) { context in
            let t = context.date.timeIntervalSinceReferenceDate
            let phase = Float(sin(t * 0.05) * 0.05)

            MeshGradient(
                width: 3,
                height: 2,
                points: [
                    SIMD2<Float>(0.00, 0.00),
                    SIMD2<Float>(0.55 + phase, -0.05),
                    SIMD2<Float>(1.00, 0.00),
                    SIMD2<Float>(0.00, 1.00),
                    SIMD2<Float>(0.45 - phase, 1.05),
                    SIMD2<Float>(1.00, 1.00)
                ],
                colors: [
                    AppColor.linen,
                    Color(red: 246/255, green: 231/255, blue: 218/255),
                    Color(red: 246/255, green: 231/255, blue: 216/255),
                    AppColor.linen,
                    Color(red: 238/255, green: 237/255, blue: 227/255),
                    AppColor.linen
                ]
            )
        }
        .ignoresSafeArea()
    }
}

/// Linen mesh sized for hero-card interiors — gives glass-over-linen depth
/// inside a panel without leaking the page background through.
struct HeroBackdrop: View {
    var body: some View {
        MeshGradient(
            width: 3,
            height: 2,
            points: [
                SIMD2<Float>(0.00, 0.00),
                SIMD2<Float>(0.50, 0.00),
                SIMD2<Float>(1.00, 0.00),
                SIMD2<Float>(0.00, 1.00),
                SIMD2<Float>(0.50, 1.00),
                SIMD2<Float>(1.00, 1.00)
            ],
            colors: [
                Color(red: 246/255, green: 231/255, blue: 218/255),
                AppColor.linen,
                Color(red: 246/255, green: 231/255, blue: 216/255),
                AppColor.linen,
                Color(red: 244/255, green: 234/255, blue: 222/255),
                Color(red: 240/255, green: 226/255, blue: 213/255)
            ]
        )
    }
}

/// Brand fill exposed as a ShapeStyle (not a `View`) so it composes with
/// `.fill(.doorGradient)` and `.foregroundStyle(.doorGradient)`.
extension ShapeStyle where Self == LinearGradient {
    static var doorGradient: LinearGradient {
        LinearGradient(
            colors: [AppColor.conifer, AppColor.coniferPressed],
            startPoint: .top,
            endPoint: .bottom
        )
    }
}
