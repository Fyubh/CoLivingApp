import SwiftUI

#if canImport(UIKit)
import UIKit
#endif

/// Glass-material input. Focus shifts the stroke to Conifer; an `hasError`
/// flag flips it to Danger so the surrounding form can drive validation
/// styling without the field knowing the error message itself.
///
/// `kind` collapses keyboard/content/secure choices into one parameter so
/// call sites stay short — passwords get an inline reveal toggle.
struct AppTextField: View {
    enum Kind { case text, email, password }

    let icon: String?
    let placeholder: String
    @Binding var text: String
    var kind: Kind = .text
    var hasError: Bool = false

    @FocusState private var focused: Bool
    @State private var revealSecure: Bool = false

    var body: some View {
        let shape = RoundedRectangle(cornerRadius: Radius.input, style: .continuous)
        let strokeColor: Color = hasError
            ? AppColor.danger
            : (focused ? AppColor.conifer.opacity(0.55) : AppColor.glassStroke)
        let strokeWidth: CGFloat = (focused || hasError) ? 1.0 : 0.5

        HStack(spacing: Spacing.s12) {
            if let icon {
                Image(systemName: icon)
                    .font(.system(size: 16, weight: .medium))
                    .foregroundStyle(focused ? AppColor.conifer : AppColor.inkSecondary)
                    .frame(width: 22, alignment: .center)
                    .animation(.spring(response: 0.28, dampingFraction: 0.78), value: focused)
            }

            field

            if kind == .password {
                Button {
                    revealSecure.toggle()
                } label: {
                    Image(systemName: revealSecure ? "eye.slash" : "eye")
                        .font(.system(size: 15, weight: .medium))
                        .foregroundStyle(AppColor.inkSecondary)
                        .frame(width: 22, height: 22)
                        .contentShape(Rectangle())
                }
                .buttonStyle(.plain)
            }
        }
        .padding(.horizontal, Spacing.s16)
        .frame(height: 54)
        .glassEffect(.regular, in: shape)
        .overlay {
            shape.strokeBorder(strokeColor, lineWidth: strokeWidth)
        }
        .animation(.spring(response: 0.28, dampingFraction: 0.78), value: focused)
        .animation(.spring(response: 0.28, dampingFraction: 0.78), value: hasError)
    }

    @ViewBuilder
    private var field: some View {
        let base = Group {
            if kind == .password && !revealSecure {
                SecureField(placeholder, text: $text)
            } else {
                TextField(placeholder, text: $text)
            }
        }
        .focused($focused)
        .appText(.body)
        .foregroundStyle(AppColor.ink)
        .tint(AppColor.conifer)
        .autocorrectionDisabled(true)

        #if canImport(UIKit)
        base
            .textInputAutocapitalization(.never)
            .keyboardType(kind == .email ? .emailAddress : .default)
            .textContentType(uiContentType)
            .submitLabel(kind == .password ? .go : .next)
        #else
        base
        #endif
    }

    #if canImport(UIKit)
    private var uiContentType: UITextContentType? {
        switch kind {
        case .email:    return .emailAddress
        case .password: return .password
        case .text:     return nil
        }
    }
    #endif
}

#Preview {
    @Previewable @State var email = ""
    @Previewable @State var password = ""

    return ZStack {
        AppBackground()
        VStack(spacing: Spacing.s16) {
            AppTextField(icon: "envelope", placeholder: "E-mail",
                         text: $email, kind: .email)
            AppTextField(icon: "lock", placeholder: "Пароль",
                         text: $password, kind: .password)
            AppTextField(icon: "lock", placeholder: "Неверный пароль",
                         text: .constant("badpass"), kind: .password, hasError: true)
        }
        .padding()
    }
}
