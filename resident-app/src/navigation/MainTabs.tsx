import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { Icon, IconName } from '../components';
import { TabBar } from '../components/TabBar';
import { CommunityScreen } from '../screens/CommunityScreen';
import { HomeScreen } from '../screens/HomeScreen';
import { NeighborsScreen } from '../screens/NeighborsScreen';
import { ProfileScreen } from '../screens/ProfileScreen';
import { ServicesScreen } from '../screens/ServicesScreen';

const Tab = createBottomTabNavigator();

function tabIcon(base: IconName, fill: IconName) {
  return ({ focused, color, size }: { focused: boolean; color: string; size: number }) => (
    <Icon name={focused ? fill : base} size={size} color={color} />
  );
}

export function MainTabs() {
  return (
    <Tab.Navigator
      tabBar={(props) => <TabBar {...props} />}
      screenOptions={{
        headerShown: false,
        sceneStyle: { backgroundColor: 'transparent' },
      }}
    >
      <Tab.Screen
        name="Home"
        component={HomeScreen}
        options={{ title: 'Главная', tabBarIcon: tabIcon('home', 'home-fill') }}
      />
      <Tab.Screen
        name="Neighbors"
        component={NeighborsScreen}
        options={{ title: 'Соседи', tabBarIcon: tabIcon('users', 'users-fill') }}
      />
      <Tab.Screen
        name="Services"
        component={ServicesScreen}
        options={{ title: 'Сервисы', tabBarIcon: tabIcon('grid', 'grid-fill') }}
      />
      <Tab.Screen
        name="Community"
        component={CommunityScreen}
        options={{ title: 'Комьюнити', tabBarIcon: tabIcon('chat', 'chat-fill') }}
      />
      <Tab.Screen
        name="Profile"
        component={ProfileScreen}
        options={{ title: 'Профиль', tabBarIcon: tabIcon('user', 'user-fill') }}
      />
    </Tab.Navigator>
  );
}
