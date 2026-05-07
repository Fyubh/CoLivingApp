import SwiftUI

struct GlassCard<Content: View>: View {
    private let cornerRadius: CGFloat
    private let padding: CGFloat
    private let tint: Color?
    private let content: Content

    init(
        cornerRadius: CGFloat = Radius.card,
        padding: CGFloat = Spacing.s24,
        tint: Color? = nil,
        @ViewBuilder content: () -> Content
    ) {
        self.cornerRadius = cornerRadius
        self.padding = padding
        self.tint = tint
        self.content = content()
    }

    var body: some View {
        let shape = RoundedRectangle(cornerRadius: cornerRadius, style: .continuous)
        return content
            .padding(padding)
            .frame(maxWidth: .infinity, alignment: .leading)
            .background {
                if let tint {
                    shape.fill(tint.opacity(0.06))
                }
            }
            .glassEffect(.regular, in: shape)
            .overlay {
                shape.strokeBorder(AppColor.glassStroke, lineWidth: 0.5)
            }
            .colivingShadow(.card)
    }
}

/// Wraps adjacent glass elements in a shared `GlassEffectContainer` so the
/// material can morph between them when their `glassEffectID`s match.
struct GlassCardGroup<Content: View>: View {
    private let spacing: CGFloat
    private let content: Content

    init(spacing: CGFloat = Spacing.s12, @ViewBuilder content: () -> Content) {
        self.spacing = spacing
        self.content = content()
    }

    var body: some View {
        GlassEffectContainer(spacing: spacing) {
            VStack(spacing: spacing) {
                content
            }
        }
    }
}

#Preview {
    ZStack {
        AppBackground()
        VStack(spacing: 16) {
            GlassCard {
                VStack(alignment: .leading, spacing: 6) {
                    Text("Квартира 12B").appText(.title)
                    Text("ул. Пражская, 14")
                        .appText(.body)
                        .foregroundStyle(AppColor.inkSecondary)
                }
            }
            GlassCard(padding: 16) {
                Text("Compact card padding")
                    .appText(.headline)
            }
            GlassCardGroup {
                GlassCard { Text("Group A").appText(.bodyMed) }
                GlassCard { Text("Group B").appText(.bodyMed) }
            }
        }
        .padding()
    }
}
