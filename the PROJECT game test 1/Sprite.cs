using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Net.Mime;
using static Top_Down_Game.InPlay;
using static Microsoft.Xna.Framework.Graphics.SpriteEffects;
using static Microsoft.Xna.Framework.Color;

namespace Top_Down_Game
{
    public class Sprite
    {
        protected Texture2D _texture;
        protected Point _size;
        protected Vector2 _position;
        protected Rectangle _source;
        protected Rectangle _collision; 
        protected Vector2 _origin;
        protected Color _colour;
        protected SpriteEffects _effects;
        protected int _width;
        protected int _height;
        protected int _halfWidth;
        protected int _halfHeight;
        protected float _scale;
        protected int _frameNumber;
        private Dictionary<string, Animation> _animations;
        protected string _currentAction;
        protected string _previousAction;
        protected bool _actionComplete;
        protected bool _busy;
        protected bool _animationComplete;
        protected float _animationSpeed;
        protected Vector2 _velocity;
        public Sprite(Texture2D texture, Vector2 position, Dictionary<string, Animation> animations)
        {
            _previousAction = "Idle";
            _currentAction = "Idle"; 
            _texture = texture; 
            _animations = animations; 
            _animationComplete = false; 
            _colour = White; 
            _effects = None; 
            _size = new Point(100, 55);
            _animationSpeed = 0.50f;
            _frameNumber = 0;
            _animationComplete = false;
            _source = AnimationFrame("Idle", ref _frameNumber);
            _width = _size.X;
            _height = _size.Y;
            _halfWidth = _width / 2;
            _halfHeight = _height / 2;
            _scale = 1.3f;
            _position = position;
            _collision = new Rectangle((int)_position.X, (int)_position.Y, _width, _height);
            _origin = new Vector2(_halfWidth + 15, _halfHeight + 27);
            _actionComplete = true;
            _busy = false;
        }
        public virtual void Update()
        {
            _previousAction = _currentAction;
            if (_animationComplete)
            {
                _actionComplete = true;
                SetAction("Idle");
            }
            if (AnimationTick)
            {
                _source = AnimationFrame(_currentAction, ref _frameNumber);
                if (_frameNumber == -1) _animationComplete = true;
                _frameNumber++;
            }
            _position += _velocity;
            _collision = new Rectangle((int)_position.X, (int)_position.Y, _width, _height);
        }
        protected void SetAction(string gameAction)
        {
            _currentAction = gameAction;
            if (_currentAction != _previousAction) StartAnimation();
        }
        public void StartAnimation()
        {
            _frameNumber = 0;
            _animationComplete = false;
        }
        public Rectangle AnimationFrame(string _gameAction, ref int frameNumber) => _animations[_gameAction].Source(ref frameNumber);
    }
}
