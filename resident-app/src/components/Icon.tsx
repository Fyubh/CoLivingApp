import Feather from '@expo/vector-icons/Feather';
import Ionicons from '@expo/vector-icons/Ionicons';
import MaterialCommunityIcons from '@expo/vector-icons/MaterialCommunityIcons';
import { T } from '../theme/tokens';

export type IconName =
  | 'home'
  | 'home-fill'
  | 'users'
  | 'users-fill'
  | 'grid'
  | 'grid-fill'
  | 'chat'
  | 'chat-fill'
  | 'user'
  | 'user-fill'
  | 'bell'
  | 'lock'
  | 'mail'
  | 'eye'
  | 'eye-off'
  | 'alert'
  | 'info'
  | 'arrow-up'
  | 'arrow-right'
  | 'chevron-right'
  | 'check'
  | 'plus'
  | 'doc'
  | 'refresh'
  | 'wifi'
  | 'droplet'
  | 'log-out'
  | 'ban'
  | 'dots'
  | 'logo-key'
  | 'door'
  | 'broom'
  | 'fridge'
  | 'shelf'
  | 'spray'
  | 'shopping'
  | 'plug'
  | 'split'
  | 'film'
  | 'dumbbell'
  | 'sparkle'
  | 'gamepad'
  | 'wrench'
  | 'tag'
  | 'calendar'
  | 'laptop'
  | 'send';

type IconLib = 'feather' | 'ion' | 'mci';

const MAP: Record<IconName, [IconLib, string]> = {
  home: ['feather', 'home'],
  'home-fill': ['ion', 'home'],
  users: ['feather', 'users'],
  'users-fill': ['ion', 'people'],
  grid: ['feather', 'grid'],
  'grid-fill': ['ion', 'grid'],
  chat: ['feather', 'message-circle'],
  'chat-fill': ['ion', 'chatbubble'],
  user: ['feather', 'user'],
  'user-fill': ['ion', 'person'],

  bell: ['feather', 'bell'],
  lock: ['feather', 'lock'],
  mail: ['feather', 'mail'],
  eye: ['feather', 'eye'],
  'eye-off': ['feather', 'eye-off'],
  alert: ['feather', 'alert-circle'],
  info: ['feather', 'info'],
  'arrow-up': ['feather', 'arrow-up'],
  'arrow-right': ['feather', 'arrow-right'],
  'chevron-right': ['feather', 'chevron-right'],
  check: ['feather', 'check'],
  plus: ['feather', 'plus'],
  doc: ['feather', 'file-text'],
  refresh: ['feather', 'refresh-cw'],
  wifi: ['feather', 'wifi'],
  droplet: ['feather', 'droplet'],
  'log-out': ['feather', 'log-out'],
  ban: ['feather', 'x-circle'],
  dots: ['feather', 'more-horizontal'],
  'logo-key': ['feather', 'key'],
  send: ['feather', 'send'],
  calendar: ['feather', 'calendar'],
  laptop: ['feather', 'monitor'],

  door: ['mci', 'door'],
  broom: ['mci', 'broom'],
  fridge: ['mci', 'fridge-outline'],
  shelf: ['mci', 'bookshelf'],
  spray: ['mci', 'spray-bottle'],
  shopping: ['mci', 'cart-outline'],
  plug: ['mci', 'power-plug-outline'],
  split: ['mci', 'call-split'],
  film: ['mci', 'movie-outline'],
  dumbbell: ['mci', 'dumbbell'],
  sparkle: ['mci', 'auto-fix'],
  gamepad: ['mci', 'gamepad-variant-outline'],
  wrench: ['mci', 'wrench-outline'],
  tag: ['mci', 'tag-outline'],
};

export function Icon({
  name,
  size = 20,
  color = T.text,
}: {
  name: IconName;
  size?: number;
  color?: string;
}) {
  const [lib, glyph] = MAP[name];
  if (lib === 'feather') return <Feather name={glyph as never} size={size} color={color} />;
  if (lib === 'ion') return <Ionicons name={glyph as never} size={size} color={color} />;
  return <MaterialCommunityIcons name={glyph as never} size={size} color={color} />;
}
