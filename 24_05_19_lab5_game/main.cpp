#include <SFML/Graphics.hpp>
#include <vector>
#include <string>

const int windowWidth = 900;
const int windowHeight = 600;
const float ballRadius = 10.f;
const float ballVelocity = 4.f;
const float paddleWidth = 120.f;
const float paddleHeight = 20.f;
const float paddleVelocity = 6.f;
const int brickRows = 2;
const int brickColumns = 12;
const float brickWidth = 60.f;
const float brickHeight = 20.f;
const float brickSpacing = 5.f;
const int unbreakableBrickRadius = 20.f;

enum class GameState { Running, Paused, Won, Lost };

struct Ball {
    sf::CircleShape shape;
    sf::Vector2f velocity{-ballVelocity, -ballVelocity};

    Ball(float x, float y) {
        shape.setPosition(x, y);
        shape.setRadius(ballRadius);
        shape.setFillColor(sf::Color::Red);
        shape.setOrigin(ballRadius, ballRadius);
    }

    void update() {
        shape.move(velocity);

        if (left() < 0) velocity.x = ballVelocity;
        else if (right() > windowWidth) velocity.x = -ballVelocity;

        if (top() < 0) velocity.y = ballVelocity;
    }

    float x() const { return shape.getPosition().x; }
    float y() const { return shape.getPosition().y; }
    float left() const { return x() - shape.getRadius(); }
    float right() const { return x() + shape.getRadius(); }
    float top() const { return y() - shape.getRadius(); }
    float bottom() const { return y() + shape.getRadius(); }
};

struct Paddle {
    sf::ConvexShape shape;
    sf::Vector2f velocity;

    Paddle(float x, float y) {
        shape.setPointCount(3);
        shape.setPoint(0, sf::Vector2f(-paddleWidth / 2, paddleHeight / 2));
        shape.setPoint(1, sf::Vector2f(paddleWidth / 2, paddleHeight / 2));
        shape.setPoint(2, sf::Vector2f(0, -paddleHeight / 2));
        shape.setPosition(x, y);
        shape.setFillColor(sf::Color::Blue);
    }

    void update() {
        shape.move(velocity);

        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Key::Left) && left() > 0)
            velocity.x = -paddleVelocity;
        else if (sf::Keyboard::isKeyPressed(sf::Keyboard::Key::Right) && right() < windowWidth)
            velocity.x = paddleVelocity;
        else
            velocity.x = 0;
    }

    float x() const { return shape.getPosition().x; }
    float y() const { return shape.getPosition().y; }
    float left() const { return x() - paddleWidth / 2.f; }
    float right() const { return x() + paddleWidth / 2.f; }
    float top() const { return y() - paddleHeight / 2.f; }
    float bottom() const { return y() + paddleHeight / 2.f; }
};

struct Brick {
    sf::RectangleShape shape;
    bool destroyed{false};

    Brick(float x, float y) {
        shape.setPosition(x, y);
        shape.setSize({brickWidth, brickHeight});
        shape.setFillColor(sf::Color::Yellow);
        shape.setOrigin(brickWidth / 2.f, brickHeight / 2.f);
    }

    float x() const { return shape.getPosition().x; }
    float y() const { return shape.getPosition().y; }
    float left() const { return x() - shape.getSize().x / 2.f; }
    float right() const { return x() + shape.getSize().x / 2.f; }
    float top() const { return y() - shape.getSize().y / 2.f; }
    float bottom() const { return y() + shape.getSize().y / 2.f; }
};

struct UnbreakableBrick {
    sf::CircleShape shape;

    UnbreakableBrick(float x, float y) {
        shape.setPosition(x, y);
        shape.setRadius(unbreakableBrickRadius);
        shape.setFillColor(sf::Color::Green);
        shape.setOrigin(unbreakableBrickRadius, unbreakableBrickRadius);
    }

    float x() const { return shape.getPosition().x; }
    float y() const { return shape.getPosition().y; }
    float left() const { return x() - shape.getRadius(); }
    float right() const { return x() + shape.getRadius(); }
    float top() const { return y() - shape.getRadius(); }
    float bottom() const { return y() + shape.getRadius(); }
};

template <class T1, class T2>
bool isIntersecting(const T1& mA, const T2& mB) {
    return mA.right() >= mB.left() && mA.left() <= mB.right() && mA.bottom() >= mB.top() && mA.top() <= mB.bottom();
}

void testCollision(Paddle& mPaddle, Ball& mBall) {
    if (!isIntersecting(mPaddle, mBall)) return;

    mBall.velocity.y = -ballVelocity;

    float paddleCenter = mPaddle.x();
    float ballCenter = mBall.x();
    float difference = ballCenter - paddleCenter;
    float percent = difference / (paddleWidth / 2);
    mBall.velocity.x = ballVelocity * percent;
}

void testCollision(Brick& mBrick, Ball& mBall) {
    if (!isIntersecting(mBrick, mBall)) return;

    mBrick.destroyed = true;

    float overlapLeft{mBall.right() - mBrick.left()};
    float overlapRight{mBrick.right() - mBall.left()};
    float overlapTop{mBall.bottom() - mBrick.top()};
    float overlapBottom{mBrick.bottom() - mBall.top()};

    bool ballFromLeft(abs(overlapLeft) < abs(overlapRight));
    bool ballFromTop(abs(overlapTop) < abs(overlapBottom));

    float minOverlapX{ballFromLeft ? overlapLeft : overlapRight};
    float minOverlapY{ballFromTop ? overlapTop : overlapBottom};

    if (abs(minOverlapX) < abs(minOverlapY))
        mBall.velocity.x = ballFromLeft ? -ballVelocity : ballVelocity;
    else
        mBall.velocity.y = ballFromTop ? -ballVelocity : ballVelocity;
}

void testCollision(UnbreakableBrick& mBrick, Ball& mBall) {
    if (!isIntersecting(mBrick, mBall)) return;

    float overlapLeft{mBall.right() - mBrick.left()};
    float overlapRight{mBrick.right() - mBall.left()};
    float overlapTop{mBall.bottom() - mBrick.top()};
    float overlapBottom{mBrick.bottom() - mBall.top()};

    bool ballFromLeft(abs(overlapLeft) < abs(overlapRight));
    bool ballFromTop(abs(overlapTop) < abs(overlapBottom));

    float minOverlapX{ballFromLeft ? overlapLeft : overlapRight};
    float minOverlapY{ballFromTop ? overlapTop : overlapBottom};

    if (abs(minOverlapX) < abs(minOverlapY))
        mBall.velocity.x = ballFromLeft ? -ballVelocity : ballVelocity;
    else
        mBall.velocity.y = ballFromTop ? -ballVelocity : ballVelocity;
}

void resetGame(std::vector<Brick>& bricks, std::vector<UnbreakableBrick>& unbreakableBricks, Ball& ball, Paddle& paddle) {
    bricks.clear();
    unbreakableBricks.clear();

    for (int iX{0}; iX < brickColumns; ++iX)
        for (int iY{0}; iY < brickRows; ++iY) {
            float x{(iX + 1) * (brickWidth + brickSpacing)};
            float y{(iY + 2) * (brickHeight + brickSpacing)};
            bricks.emplace_back(x, y);
        }

    unbreakableBricks.emplace_back(windowWidth / 4, windowHeight / 3);
    unbreakableBricks.emplace_back(2 * windowWidth / 3, windowHeight / 3);
    unbreakableBricks.emplace_back(windowWidth / 2, windowHeight / 2);

    ball.shape.setPosition(windowWidth / 2, windowHeight / 2);
    ball.velocity = {-ballVelocity, -ballVelocity};
    paddle.shape.setPosition(windowWidth / 2, windowHeight - 50);
}

int main() {
    sf::Font font;
    if (!font.loadFromFile("arial.ttf")) {
        return -1;
    }

    GameState gameState = GameState::Running;

    Ball ball{windowWidth / 2, windowHeight / 2};
    Paddle paddle{windowWidth / 2, windowHeight - 50};

    std::vector<Brick> bricks;
    std::vector<UnbreakableBrick> unbreakableBricks;

    resetGame(bricks, unbreakableBricks, ball, paddle);

    sf::Text pauseText("Pause", font, 20);
    pauseText.setPosition(windowWidth - 100, 10);
    pauseText.setFillColor(sf::Color::Black);
    pauseText.setStyle(sf::Text::Bold | sf::Text::Underlined);

    sf::RectangleShape pauseButton(sf::Vector2f(80, 30));
    pauseButton.setPosition(windowWidth - 110, 10);
    pauseButton.setFillColor(sf::Color::Red);

    sf::Text restartText("Restart", font, 20);
    restartText.setPosition(windowWidth - 100, 50);
    restartText.setFillColor(sf::Color::Black);
    restartText.setStyle(sf::Text::Bold | sf::Text::Underlined);

    sf::RectangleShape restartButton(sf::Vector2f(80, 30));
    restartButton.setPosition(windowWidth - 110, 50);
    restartButton.setFillColor(sf::Color::Red);

    sf::Text endText("", font, 50);
    endText.setFillColor(sf::Color::White);
    endText.setPosition(windowWidth / 2 - 100, windowHeight / 2 - 50);

    sf::RenderWindow window{{windowWidth, windowHeight}, "Arkanoid"};
    window.setFramerateLimit(60);

    while (window.isOpen()) {
        sf::Event event;
        while (window.pollEvent(event)) {
            if (event.type == sf::Event::Closed) window.close();
            if (event.type == sf::Event::MouseButtonPressed) {
                if (event.mouseButton.button == sf::Mouse::Left) {
                    if (pauseButton.getGlobalBounds().contains(event.mouseButton.x, event.mouseButton.y)) {
                        gameState = gameState == GameState::Paused ? GameState::Running : GameState::Paused;
                    } else if (restartButton.getGlobalBounds().contains(event.mouseButton.x, event.mouseButton.y)) {
                        resetGame(bricks, unbreakableBricks, ball, paddle);
                        gameState = GameState::Running;
                    }
                }
            }
        }

        if (gameState == GameState::Running) {
            ball.update();
            paddle.update();
            testCollision(paddle, ball);

            for (auto& brick : bricks) testCollision(brick, ball);
            for (auto& uBrick : unbreakableBricks) testCollision(uBrick, ball);

            bricks.erase(remove_if(begin(bricks), end(bricks), [](const Brick& mBrick) { return mBrick.destroyed; }), end(bricks));

            if (bricks.empty()) {
                gameState = GameState::Won;
                endText.setString("You Win!");
            }

            if (ball.bottom() > windowHeight) {
                gameState = GameState::Lost;
                endText.setString("You Lost!");
            }
        }

        window.clear(sf::Color::Black);

        window.draw(ball.shape);
        window.draw(paddle.shape);
        for (auto& brick : bricks) window.draw(brick.shape);
        for (auto& uBrick : unbreakableBricks) window.draw(uBrick.shape);

        window.draw(pauseButton);
        window.draw(pauseText);
        window.draw(restartButton);
        window.draw(restartText);

        if (gameState == GameState::Won || gameState == GameState::Lost) {
            window.draw(endText);
        }

        window.display();
    }

    return 0;
}
