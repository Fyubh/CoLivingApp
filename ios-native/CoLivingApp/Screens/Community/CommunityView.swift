import SwiftUI

/// Phase 2 placeholder. Likely hidden behind a feature flag for first
/// TestFlight (Чаты / Ивенты / Барахолка need real moderation/report/block
/// before they can pass App Review).
struct CommunityView: View {
    var body: some View {
        NavigationStack {
            TabPlaceholderBody(
                icon: "bubble.left.and.bubble.right",
                hint: "Чаты, ивенты и барахолка дома."
            )
            .navigationTitle("Сообщество")
        }
    }
}

#Preview {
    CommunityView()
}
