import SwiftUI

/// Phase 2 placeholder. Phase 3 fills this with the apartment context card,
/// notifications list, DoorCard, and pull-to-refresh. The `NavigationStack`
/// is owned by the tab so push/pop state is local to Home.
struct HomeView: View {
    var body: some View {
        NavigationStack {
            TabPlaceholderBody(
                icon: "house",
                hint: "Контекст здания, уведомления и быстрые действия."
            )
            .navigationTitle("Главная")
        }
    }
}

#Preview {
    HomeView()
}
