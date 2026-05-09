import SwiftUI

/// 1–5 star rating widget — read-only when `onChange` is nil, tappable
/// otherwise. Scaling is via SF Symbols (`star.fill` / `star`) so weight
/// matches the surrounding ink.
struct StarRating: View {
    let value: Int
    let size: CGFloat
    var onChange: ((Int) -> Void)? = nil

    init(value: Int, size: CGFloat = 22, onChange: ((Int) -> Void)? = nil) {
        self.value = value
        self.size = size
        self.onChange = onChange
    }

    var body: some View {
        HStack(spacing: Spacing.s8) {
            ForEach(1...5, id: \.self) { i in
                star(for: i)
            }
        }
    }

    @ViewBuilder
    private func star(for i: Int) -> some View {
        let filled = i <= value
        let symbol = Image(systemName: filled ? "star.fill" : "star")
            .font(.system(size: size, weight: .regular))
            .foregroundStyle(filled ? AppColor.warning : AppColor.inkTertiary)

        if let onChange {
            Button {
                onChange(i)
            } label: {
                symbol.contentShape(Rectangle())
            }
            .buttonStyle(.plain)
        } else {
            symbol
        }
    }
}

#Preview {
    @Previewable @State var rating = 3
    return ZStack {
        AppBackground()
        VStack(spacing: 16) {
            StarRating(value: 4)              // read-only
            StarRating(value: rating) { rating = $0 }
            StarRating(value: 0, size: 28) { rating = $0 }
        }
        .padding()
    }
}
