import SwiftUI

struct SectionHeader<Trailing: View>: View {
    private let title: String
    private let trailing: Trailing

    init(_ title: String, @ViewBuilder trailing: () -> Trailing = { EmptyView() }) {
        self.title = title
        self.trailing = trailing()
    }

    var body: some View {
        HStack(alignment: .firstTextBaseline, spacing: Spacing.s8) {
            Text(title)
                .appText(.capUpper)
                .foregroundStyle(AppColor.ink)
                .lineLimit(1)
                .frame(maxWidth: .infinity, alignment: .leading)
            trailing
        }
        .padding(.bottom, Spacing.s12)
    }
}

#Preview {
    ZStack {
        AppBackground()
        VStack(alignment: .leading, spacing: Spacing.s18) {
            SectionHeader("Уведомления")
            SectionHeader("Услуги") {
                Button("все") {}
                    .font(.footnote.weight(.medium))
                    .buttonStyle(.plain)
                    .foregroundStyle(AppColor.conifer)
            }
        }
        .padding()
    }
}
