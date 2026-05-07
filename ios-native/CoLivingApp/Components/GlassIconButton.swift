import SwiftUI

/// 44pt glass icon button. Tactile press: translateY + brightness shift,
/// no scale (Apple's nav/tab icons don't scale on tap — only "fingerable"
/// surfaces do). Optional `glassEffectID` + `namespace` enable native morph
/// when the button lives inside a `GlassEffectContainer`.
struct GlassIconButton: View {
    private let icon: String
    private let badge: Bool
    private let glassEffectID: String?
    private let namespace: Namespace.ID?
    private let action: () -> Void

    init(
        icon: String,
        badge: Bool = false,
        glassEffectID: String? = nil,
        namespace: Namespace.ID? = nil,
        action: @escaping () -> Void = {}
    ) {
        self.icon = icon
        self.badge = badge
        self.glassEffectID = glassEffectID
        self.namespace = namespace
        self.action = action
    }

    var body: some View {
        Button(action: action) {
            Image(systemName: icon)
                .font(.system(size: 17, weight: .medium))
                .foregroundStyle(AppColor.ink)
                .frame(width: 44, height: 44)
                .glassEffect(.regular, in: Circle())
                .modifier(OptionalGlassEffectID(id: glassEffectID, namespace: namespace))
                .overlay {
                    Circle().strokeBorder(AppColor.glassStroke, lineWidth: 0.5)
                }
                .overlay(alignment: .topTrailing) {
                    if badge {
                        Circle()
                            .fill(AppColor.clay)
                            .frame(width: 9, height: 9)
                            .overlay(Circle().stroke(.white, lineWidth: 1.5))
                            .offset(x: 1, y: -1)
                    }
                }
        }
        .buttonStyle(IconButtonPressStyle())
    }
}

private struct IconButtonPressStyle: ButtonStyle {
    func makeBody(configuration: Configuration) -> some View {
        configuration.label
            .offset(y: configuration.isPressed ? 0.5 : 0)
            .brightness(configuration.isPressed ? -0.04 : 0)
            .opacity(configuration.isPressed ? 0.92 : 1)
            .animation(.spring(response: 0.18, dampingFraction: 0.75),
                       value: configuration.isPressed)
    }
}

private struct OptionalGlassEffectID: ViewModifier {
    let id: String?
    let namespace: Namespace.ID?

    func body(content: Content) -> some View {
        if let id, let namespace {
            content.glassEffectID(id, in: namespace)
        } else {
            content
        }
    }
}

#Preview {
    @Previewable @Namespace var ns
    ZStack {
        AppBackground()
        HStack(spacing: 16) {
            GlassIconButton(icon: "bell.fill")
            GlassIconButton(icon: "bell.fill", badge: true)
            GlassIconButton(icon: "gearshape.fill",
                            glassEffectID: "gear", namespace: ns)
            GlassIconButton(icon: "person.fill",
                            glassEffectID: "person", namespace: ns)
        }
    }
}
