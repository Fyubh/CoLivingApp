import SwiftUI

/// Calm empty state for residents who are signed in but not yet assigned to
/// an apartment. Backend returns `200 OK` with body `null` from
/// `/Apartments/my-context` for these users; HomeView falls through to this
/// view instead of an error. Wrapped in a refreshable ScrollView so the user
/// can pull to retry once the manager onboards them.
struct EmptyApartmentState: View {
    let onRefresh: () async -> Void

    var body: some View {
        ScrollView {
            VStack(spacing: Spacing.s16) {
                Image(systemName: "house")
                    .font(.system(size: 36, weight: .light))
                    .foregroundStyle(AppColor.inkSecondary)
                    .padding(.top, Spacing.s32)

                Text("Вы пока не заселены")
                    .appText(.title)
                    .foregroundStyle(AppColor.ink)
                    .multilineTextAlignment(.center)

                Text("Как только менеджер дома закрепит за вами квартиру, она появится здесь.")
                    .appText(.body)
                    .foregroundStyle(AppColor.inkSecondary)
                    .multilineTextAlignment(.center)
                    .padding(.horizontal, Spacing.s24)
            }
            .frame(maxWidth: .infinity)
            .padding(.top, Spacing.s32)
        }
        .refreshable { await onRefresh() }
    }
}

#Preview {
    ZStack {
        AppBackground()
        EmptyApartmentState(onRefresh: {})
    }
}
